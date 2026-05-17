using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Enums;
using SPA_приложение.Exceptions;
using SPA_приложение.Models;

namespace SPA_приложение.Services
{
    public interface ICommentFileService
    {
        Task Create(IFormFile file, Comment comment);
        Task<ILookup<int, CommentFile>> GetByCommentIds(List<int> commentIds);
    };
    public class CommentFileService : ICommentFileService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CommentFileService(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task Create(IFormFile file, Comment comment)
        {
            var fileType = GetFileType(file);

            string filePath = fileType switch
            {
            FileType.Text => await SaveFile(file),

            FileType.JPG => await SaveImage(file, fileType),

            _ => throw new InvalidFileException($"files[{0}]", "Unsupported format")
            };

            var commentFile = new CommentFile(comment.Id, filePath, file.FileName, file.Length, comment, fileType);

            _db.CommentsFiles.Add(commentFile);
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
        private static FileType GetFileType(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName)
                .ToLower();

            return ext switch
            {
                ".jpg" => FileType.JPG,
                ".png" => FileType.JPG,
                ".gif" => FileType.JPG,

                ".txt" => FileType.Text,

                _ => throw new InvalidFileException($"files[{0}]", "Unsupported format")
            };
        }

        private async Task<string> SaveImage(IFormFile file, FileType fyleType)
        {
            (string fullPath, string fileName) = GetFullPath(file.FileName);
            var imageBytes = await ResizeImageIfNeeded(file, fyleType);

            await File.WriteAllBytesAsync(fullPath, imageBytes);
            return "/uploads/" + fileName;
        }
        private static async Task<byte[]> ResizeImageIfNeeded(IFormFile file, FileType fyleType)
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());

            if (image.Width <= 320 && image.Height <= 240)
            {
                using var original = new MemoryStream();

                await file.CopyToAsync(original);

                return original.ToArray();
            }

            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,

                    Size = new Size(320, 240)
                }));

            using var output = new MemoryStream();

            switch (fyleType)
            {
                case FileType.JPG:
                    await image.SaveAsJpegAsync(output);
                    break;

                case FileType.PNG:
                    await image.SaveAsPngAsync(output);
                    break;

                case FileType.GIF:
                    await image.SaveAsGifAsync(output);
                    break;

                default:
                    throw new InvalidFileException($"files[{0}]", "Unsupported format");
            }

            return output.ToArray();
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
