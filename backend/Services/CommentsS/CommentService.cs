using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SPA_app.Constants;
using SPA_app.Enums;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.Hubs;
using SPA_app.Services.CacheS;
using SPA_app.Services.CaptchaS;
using SPA_app.Services.FileS;
using SPA_приложение.Constants;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Models;

namespace SPA_app.Services.CommentsS
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _db;
        private readonly ICommentFileService _fileService;
        private readonly ICaptchaService _captchaService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheService _cacheService;
        public CommentService(AppDbContext db, IHubContext<CommentsHub> hub, ICommentFileService fileService, ICaptchaService captchaService, IEventPublisher eventPublisher, ICacheService cacheService)
        {
            _db = db;
            _fileService = fileService;
            _captchaService = captchaService;
            _eventPublisher = eventPublisher;
            _cacheService = cacheService;
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

            await _fileService.CreateMany(dto.Files, comment);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            var comentDTO = new CommentDTO(comment);

            await _eventPublisher.Publish(new CommentCreatedEvent(comentDTO));

            return comentDTO;
        }

        public async Task<CommentsPageDTO> GetComments(int page, CommentSorting sort, bool desc)
        {
            string cacheKey = CacheKeys.CommentsCacheKey(page, sort, desc);
            var cached = await _cacheService.GetAsync<CommentsPageDTO>(cacheKey);

            if (cached is not null)
                return cached;

            var result = await Get(page, sort, desc);

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1));
            return result;
        }
        public async Task<CommentsPageDTO> Get(int page, CommentSorting sort, bool desc)
        {
            int pageSize = PaginationConstants.DefaultPageSize;

            int offset = page * pageSize;

            var query = _db.Comments
                .AsNoTracking()
                .Where(x => x.ParentId == null);

            query = sort switch
            {
                CommentSorting.CreatedAt =>
                    desc
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),

                CommentSorting.UserName =>
                    desc
                        ? query.OrderByDescending(x => x.UserName)
                        : query.OrderBy(x => x.UserName),

                CommentSorting.Email =>
                    desc
                        ? query.OrderByDescending(x => x.Email)
                        : query.OrderBy(x => x.Email),

                _ => query.OrderByDescending(x => x.CreatedAt)
            };

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

            var tree = BuildTree(childrenLookup, fileDtoDict, sort, desc);

            return new CommentsPageDTO
            {
                Items = tree,
                HasNextPage = hasNextPage
            };
        }

        private static List<CommentDTO> BuildTree(
            ILookup<int?, Comment> childrenLookup,
            Dictionary<int, List<CommentFileDTO>> fileDtoDict,
            CommentSorting sort,
            bool desc,
            int? parentId = null)
        {
            var children = sort switch
            {
                CommentSorting.CreatedAt => desc
                        ? childrenLookup[parentId].OrderByDescending(x => x.CreatedAt)
                        : childrenLookup[parentId].OrderBy(x => x.CreatedAt),

                CommentSorting.UserName => desc
                        ? childrenLookup[parentId].OrderByDescending(x => x.UserName)
                        : childrenLookup[parentId].OrderBy(x => x.UserName),

                CommentSorting.Email => desc
                        ? childrenLookup[parentId].OrderByDescending(x => x.Email)
                        : childrenLookup[parentId].OrderBy(x => x.Email),

                _ => childrenLookup[parentId].OrderByDescending(x => x.CreatedAt)
            };

            return children
                .Select(x => new CommentDTO(
                    x,
                    fileDtoDict.GetValueOrDefault(x.Id, []),

                    BuildTree(
                        childrenLookup,
                        fileDtoDict,
                        CommentSorting.CreatedAt,
                        false,
                        x.Id
                    )))
                .ToList();
        }
    }
}
