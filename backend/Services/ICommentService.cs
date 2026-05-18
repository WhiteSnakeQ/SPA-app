using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPA_app.Hubs;
using SPA_приложение.Constants;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Enums;
using SPA_приложение.Helpers;
using SPA_приложение.Models;
using System.Linq;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SPA_приложение.Services
{
    public interface ICommentService
    {
        Task<CommentDTO> Create(CreateCommentDTO dto);
        Task<CommentsPageDTO> Get(int page, string sort, bool desc);
    };

    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<CommentsHub> _hub;
        private readonly ICommentFileService _fileService;
        private readonly ICaptchaService _captchaService;

        public CommentService(AppDbContext db, IHubContext<CommentsHub> hub, ICommentFileService fileService, ICaptchaService captchaService)
        {
            _db = db;
            _hub = hub;
            _fileService = fileService;
            _captchaService = captchaService;
        }

        public async Task<CommentDTO> Create(CreateCommentDTO dto)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            await _captchaService.Validate(dto.CaptchaId, dto.CaptchaAnswer);

            var comment = new Comment(dto);

            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();

            if (dto.ParentId == null)
                comment.RootId = comment.Id;
            else
            {
                comment.RootId = await _db.Comments
                    .Where(x => x.Id == dto.ParentId)
                    .Select(x => x.RootId)
                    .FirstAsync();
            }

            foreach (var file in dto.Files ?? [])
                await _fileService.Create(file, comment);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            if (comment.ParentId == null)
                await _hub.Clients.All.SendAsync("CommentCreated", new CommentDTO(comment));
            else
                await _hub.Clients.All.SendAsync("ReplyCreated", new CommentDTO(comment));
            return new CommentDTO(comment);
        }

        public async Task<CommentsPageDTO> Get(int page, string sort, bool desc)
        {
            int pageSize = PaginationConstants.DefaultPageSize;

            int offset = page * pageSize;

            Expression<Func<Comment, object>> keySelector = sort switch
            {
                "username" => x => x.UserName,
                "email" => x => x.Email,
                _ => x => x.CreatedAt
            };

            var query = _db.Comments
                .AsNoTracking()
                .Where(x => x.ParentId == null);

            query = desc
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);

            var roots = await query
                .Skip(offset)
                .Take(pageSize + 1)
                .ToListAsync();

            bool hasNextPage = roots.Count > pageSize;

            roots = roots
                .Take(pageSize)
                .ToList();

            if (roots.Count <= 0)
            {
                return new CommentsPageDTO
                {
                    Items = new List<CommentDTO>(),
                    HasNextPage = false
                };
            }

            var rootIds = roots
                .Select(x => x.RootId)
                .ToList();

            var comments = await _db.Comments
                .AsNoTracking()
                .Where(c => rootIds.Contains(c.RootId))
                .ToListAsync();

            var childrenLookup = comments
                .ToLookup(x => x.ParentId);

            var commentIds = comments
                .Select(x => x.Id)
                .ToList();

            var filesLookup = await _fileService.GetByCommentIds(commentIds);
            var fileDtoDict = filesLookup.ToDictionary(x => x.Key, x => x
                .Select(f => new CommentFileDTO(f))
                .ToList());

            var tree = BuildTree(childrenLookup, fileDtoDict, keySelector.Compile(), desc);
            
            return new CommentsPageDTO
            {
                Items = tree,
                HasNextPage = hasNextPage
            };
        }

        private static List<CommentDTO> BuildTree(
            ILookup<int?, Comment> childrenLookup,
            Dictionary<int, List<CommentFileDTO>> fileDtoDict,
            Func<Comment, object> keySelector,
            bool desc,
            int? parentId = null)
        {
            var children = desc
                ? childrenLookup[parentId]
                    .OrderByDescending(keySelector)

                : childrenLookup[parentId]
                    .OrderBy(keySelector);

            return children
                .Select(x => new CommentDTO(
                    x,
                    fileDtoDict.GetValueOrDefault(x.Id, []),

                    BuildTree(
                        childrenLookup,
                        fileDtoDict,
                        x => x.CreatedAt,
                        false,
                        x.Id
                    )))
                .ToList();
        }
    }
}
    
