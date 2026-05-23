using SPA_app.Models.ElasticSearchDocuments;
using SPA_приложение.DTOs;
using SPA_приложение.Models;

namespace SPA_app.Services.ElasticSearch
{
    public interface ICommentSearchService
    {
        Task IndexComment(CommentDTO comment);
        Task<List<CommentSearchDocument>> Search(string text);
        Task BulkIndexComments(List<CommentSearchDocument> documents);
    }
}
