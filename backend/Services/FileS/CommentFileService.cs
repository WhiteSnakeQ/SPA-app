using Microsoft.EntityFrameworkCore;
using SPA_app.Events.FileUploaded;
using SPA_app.Events.Interface;
using SPA_app.RabbitMQ.Messages;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Enums;
using SPA_приложение.Exceptions;
using SPA_приложение.Models;
using System.Diagnostics;
using System.Threading.Channels;

namespace SPA_app.Services.FileS
{
	public class CommentFileService : ICommentFileService
	{
		private readonly AppDbContext _db;
		private readonly IWebHostEnvironment _env;
		private readonly Channel<FileCreatedMessage> _channel;
		public CommentFileService(AppDbContext db, IWebHostEnvironment env, Channel<FileCreatedMessage> channel)
		{
			_db = db;
			_env = env;
			_channel = channel;
        }
		
		public async Task<string> Create(IFormFile file, Comment comment)
		{
            var ext = System.IO.Path.GetExtension(file.FileName).ToLower();

			var fileType = GetFileType(file, ext);

			string filePath = await SaveFile(file);

			var commentFile = new CommentFile(comment.Id, filePath, file.FileName, file.Length, comment, fileType);

            _channel.Writer.TryWrite(
                new FileCreatedMessage
                {
                    File = new CommentFileDTO(commentFile, comment.Id, file.Length, fileType),
					FileExt = ext,
                });

            return filePath;
		}

		public async Task CreateMany(List<IFormFile>? files, Comment comment)
		{
			var savedFiles = new List<string>();
			try
			{
                var tasks = files?.Select(file => Create(file, comment)) ?? Array.Empty<Task<string>>();
                savedFiles = (await Task.WhenAll(tasks)).ToList();
            }
			catch (Exception)
			{
				foreach (var filePath in savedFiles)
				{
					var physicalPath = System.IO.Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
					if (File.Exists(physicalPath))
						File.Delete(physicalPath);
				}
				throw;
			}
		}
		public async Task<ILookup<int, CommentFile>> GetByCommentIds(List<int> commentIds)
		{
			var files = await _db.CommentsFiles
				.Where(x => commentIds.Contains(x.CommentId))
				.ToListAsync();

			return files.ToLookup(x => x.CommentId);
		}

        private async Task<string> SaveFile(IFormFile file)
        {
            (string fullPath, string fileName) = GetFullPath(file.FileName);

            var directory = System.IO.Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            await using var stream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true
            );

            await file.CopyToAsync(stream);

            return "/uploads/" + fileName;
        }
        private static FileType GetFileType(IFormFile file, string ext)
		{


			return ext switch
			{
				".jpg" => FileType.Image,
				".png" => FileType.Image,
				".gif" => FileType.Image,

				".txt" => FileType.Text,

				_ => throw new InvalidFileException($"files[{0}]", "Unsupported format")
			};
		}

		private (string fullPath, string filename) GetFullPath(string filename)
		{
			var uploadsFolder = System.IO.Path.Combine(_env.WebRootPath, "uploads");
			Directory.CreateDirectory(uploadsFolder);

			var _fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(filename);

			return (System.IO.Path.Combine(uploadsFolder, _fileName), _fileName);
		}
	}
}
