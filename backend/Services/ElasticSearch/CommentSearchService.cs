using Elastic.Clients.Elasticsearch;
using SPA_app.Models.ElasticSearchDocuments;
using SPA_приложение.Models;

namespace SPA_app.Services.ElasticSearch
{
    public class CommentSearchService : ICommentSearchService
    {
        private readonly ElasticsearchClient _client;

        public CommentSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public async Task IndexComment(Comment comment)
        {
            var document = new CommentSearchDocument(comment.Id, comment.UserName, comment.Email, comment.Text);

            var response = await _client.IndexAsync(
                document,
                idx => idx.Index("comments")
            );
        }

        public async Task<List<CommentSearchDocument>> Search(string text)
        {
            var response = await _client.SearchAsync<CommentSearchDocument>(
                s => s
                    .Indices("comments")
                    .Query(q => q
                        .MultiMatch(mm => mm
                            .Query(text)
                            .Fields(new[]
                            {
                                Infer.Field<CommentSearchDocument>(x => x.Text),
                                Infer.Field<CommentSearchDocument>(x => x.UserName),
                                Infer.Field<CommentSearchDocument>(x => x.Email)
                            })
                        )
                    )
            );
            if (!response.IsValidResponse)
                return [];
            return response.Documents.ToList();
        }
    }
}
