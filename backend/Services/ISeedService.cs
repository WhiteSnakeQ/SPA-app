using SPA_приложение.Data;
using SPA_приложение.DTOs;
using SPA_приложение.Models;

namespace SPA_app.Services
{
    public interface ISeedService
    {
        Task<List<Comment>?> SeedComments(int number = 35);
    }

    public class SeedService : ISeedService
    {
        private readonly AppDbContext _db;
        public SeedService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<List<Comment>?> SeedComments(int count = 35)
        {
            if (_db.Comments.Any())
                return null;

            var roots = new List<Comment>();

            for (int i = 1; i <= count; i++)
            {
                var comment = new Comment
                (
                    $"User{i}",
                    $"user{i}@mail.com",
                    $"Test comment {i}",
                    DateTime.UtcNow.AddMinutes(-i)
                );

                roots.Add(comment);
            }

            _db.Comments.AddRange(roots);

            await _db.SaveChangesAsync();

            foreach (var comment in roots)
            {
                comment.RootId = comment.Id;
            }

            await _db.SaveChangesAsync();
            return roots;
        }
    }
}
