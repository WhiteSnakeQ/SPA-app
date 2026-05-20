using SPA_app.Models.ElasticSearchDocuments;
using SPA_приложение.Models;

namespace SPA_app.Services.ElasticSearch
{
    public interface ICommentSearchService
    {
        Task IndexComment(Comment comment);
        Task<List<CommentSearchDocument>> Search(string text);
    }
}
