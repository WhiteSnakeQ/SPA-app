using Ganss.Xss;
using GreenDonut;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPA_app.BackGroundChannel.Comments;
using SPA_app.Constants;
using SPA_app.Enums;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.Hubs;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using SPA_app.Services.CacheS;
using SPA_app.Services.CaptchaS;
using SPA_app.Services.FileS;
using SPA_приложение.Constants;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Helpers;
using SPA_приложение.Models;
using System.Diagnostics;
using System.Threading.Channels;

namespace SPA_app.Services.CommentsS
{
	public class CommentService : ICommentService
	{
		private readonly AppDbContext _db;
		private readonly ICommentFileService _fileService;
		private readonly ICaptchaService _captchaService;
		private readonly ICacheService _cacheService;
		private readonly Channel<CommentCreatedMessage> _channel;
		public CommentService(AppDbContext db, ICommentFileService fileService, ICaptchaService captchaService, 
			 ICacheService cacheService, Channel<CommentCreatedMessage> channel)
		{
			_db = db;
			_fileService = fileService;
			_captchaService = captchaService;
			_cacheService = cacheService;
			_channel = channel;
		}

		public async Task<int> Create(CreateCommentDTO dto)
		{

            await _captchaService.Validate(dto.CaptchaId, dto.CaptchaAnswer);

            if (dto.RootId == Guid.Empty)
            {
                dto.RootId = Guid.NewGuid();
            }

            var comment = new Comment(
				dto.UserName,
				dto.Email,
				dto.Homepage,
				dto.RequestId,
				dto.RootId,
				HtmlSanitizerHelper.Sanitize(dto.Text),
				dto.ParentId
			);

			try
			{
				_db.Comments.Add(comment);

				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&	(sql.Number == 2601 || sql.Number == 2627))
			{
				return await _db.Comments
					.AsNoTracking()
					.Where(x => x.RequestId == comment.RequestId)
					.Select(x => x.Id)
					.FirstAsync();
			}

            try
			{
				await _fileService.CreateMany(dto.Files, comment);
				await _db.SaveChangesAsync();
			}
			catch (Exception)
			{
				_db.Comments.Remove(comment);
				await _db.SaveChangesAsync();
				throw;
			}

			_channel.Writer.TryWrite(
				new CommentCreatedMessage
				{
					Comment = new CommentDTO(comment)
				});

            return comment.Id;
		}

		public async Task<CommentsPageDTO> GetCommentsCache(int page, CommentSorting sort, bool desc)
		{
			string cacheKey = CacheKeys.CommentsCacheKey(page, sort, desc);
			var cached = await _cacheService.GetAsync<CommentsPageDTO>(cacheKey);

			if (cached is not null)
				return cached;

			var result = await Get(page, sort, desc);
			
			await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
			return result;
		}

		public async Task<List<CommentDTO>> GetReplyCache(int commentId)
		{
			string cacheKey = CacheKeys.ReplyCacheKey(commentId);
			var cached = await _cacheService.GetAsync<List<CommentDTO>>(cacheKey);

			if (cached is not null)
				return cached;

			var result = await GetReply(commentId);

			await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
			return result;
		}

		public async Task<List<CommentDTO>> GetReply(int commentId)
		{
			var childrens = await _db.Comments
				.AsNoTracking()
				.Where(x => x.ParentId == commentId)
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();

			var commentIds = childrens.Select(x => x.Id).ToList();

			var filesLookup = await _fileService.GetByCommentIds(commentIds);
			var fileDtoDict = filesLookup.ToDictionary(x => x.Key, x => x
				.Select(f => new CommentFileDTO(f))
				.ToList());

			var replyCounts = await _db.Comments
                .AsNoTracking()
                .Where(x => x.ParentId != null)
				.GroupBy(x => x.ParentId)
				.ToDictionaryAsync(
					g => g.Key!.Value,
					g => g.Count());

			return childrens
				.Select(x =>
				{
					var dto = new CommentDTO(x);

					dto.Files = fileDtoDict.GetValueOrDefault(x.Id, new List<CommentFileDTO>());
					dto.ReplyCount = replyCounts.GetValueOrDefault(x.Id, 0);
					return dto;
				})
				.ToList();
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

				CommentSorting.userName =>
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
				.Select(x => x.Id)
				.ToList();

			var replyCountsDict = await _db.Comments.Where(x =>
				x.ParentId != null
				&& rootIds.Contains(x.ParentId.Value))
					.GroupBy(x => x.ParentId)
					.Select(g => new
					{
						ParentId = g.Key!.Value,

						Count = g.Count()
					})
					.ToDictionaryAsync(
						x => x.ParentId,
						x => x.Count);

			//var comments = await _db.Comments
			//	.AsNoTracking()
			//	.Where(c => c.ParentId != null && rootIds.Contains(c.ParentId.Value))
			//	.ToListAsync();

			//var childrenLookup = comments
			//	.ToLookup(x => x.ParentId);

			//var commentIds = comments
			//	.Select(x => x.Id)
			//	.ToList();

			var filesLookup = await _fileService.GetByCommentIds(rootIds);
			var fileDtoDict = filesLookup.ToDictionary(x => x.Key, x => x
				.Select(f => new CommentFileDTO(f))
				.ToList());

            var commentsDTO = roots
				.Select(r =>  new CommentDTO(r, fileDtoDict.GetValueOrDefault(r.Id, []), null, replyCountsDict.GetValueOrDefault(r.Id, 0)))
				.ToList();

            //var tree = BuildTree(childrenLookup, fileDtoDict, sort, desc, replyCountsDict);
			
			return new CommentsPageDTO
			{
				Items = commentsDTO,
				HasNextPage = hasNextPage
			};
		}
		

		private static List<CommentDTO> BuildTree(
			ILookup<int?, Comment> childrenLookup,
			Dictionary<int, List<CommentFileDTO>> fileDtoDict,
			CommentSorting sort,
			bool desc,
			Dictionary<int, int> replyCountsDict,
			int? parentId = null)
		{
			var children = sort switch
			{
				CommentSorting.CreatedAt => desc
						? childrenLookup[parentId].OrderByDescending(x => x.CreatedAt)
						: childrenLookup[parentId].OrderBy(x => x.CreatedAt),

				CommentSorting.userName => desc
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
						replyCountsDict,
						x.Id
					),
					replyCountsDict.GetValueOrDefault(x.Id, 0)))
				.ToList();
		}
	}
}
