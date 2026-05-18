using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPA_app.Events.FileUploaded;
using SPA_app.Events.Interface;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Enums;
using SPA_приложение.Exceptions;
using SPA_приложение.Models;

namespace SPA_приложение.Services
{
    public interface ICommentFileService
    {
        Task<string> Create(IFormFile file, Comment comment);
        Task CreateMany(List<IFormFile>? file, Comment comment);
        Task<ILookup<int, CommentFile>> GetByCommentIds(List<int> commentIds);
    };
    public class CommentFileService : ICommentFileService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IEventPublisher _eventPublisher;
        public CommentFileService(AppDbContext db, IWebHostEnvironment env, IEventPublisher eventPublisher)
        {
            _db = db;
            _env = env;
            _eventPublisher = eventPublisher;
        }

        public async Task<string> Create(IFormFile file, Comment comment)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            var fileType = GetFileType(file, ext);

            string filePath = await SaveFile(file);

            var commentFile = new CommentFile(comment.Id, filePath, file.FileName, file.Length, comment, fileType);

            _db.CommentsFiles.Add(commentFile);
            await _eventPublisher.Publish(new FileUploadedEvent(filePath, fileType, ext));

            return filePath;
        }

        public async Task CreateMany(List<IFormFile>? files, Comment comment)
        {
            var savedFiles = new List<string>();
            try
            {
                foreach (var file in files ?? [])
                {
                    var filePath = await Create(file, comment);

                    savedFiles.Add(filePath);
                }
            }
            catch (Exception)
            {
                foreach (var filePath in savedFiles)
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
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
            using var stream = new FileStream(fullPath, FileMode.Create);

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
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var _fileName = Guid.NewGuid().ToString() + Path.GetExtension(filename);

            return (Path.Combine(uploadsFolder, _fileName), _fileName);
        }
    }
}
