using SPA_приложение.Models;

namespace SPA_app.Services.FileS
{
    public interface ICommentFileService
    {
        Task<string> Create(IFormFile file, Comment comment);
        Task CreateMany(List<IFormFile>? file, Comment comment);
        Task<ILookup<int, CommentFile>> GetByCommentIds(List<int> commentIds);
    };
}
