using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using SPA_app.Models.ElasticSearchDocuments;
using SPA_приложение.DTOs;

namespace SPA_app.Services.ElasticSearch
{
	public class CommentSearchService : ICommentSearchService
	{
		private readonly ElasticsearchClient _client;

		public CommentSearchService(ElasticsearchClient client)
		{
			_client = client;
		}

		public async Task IndexComment(CommentDTO comment)
		{
			var document = new CommentSearchDocument(comment.Id, comment.UserName, comment.Email, comment.Text);

			var response = await _client.IndexAsync(document,
				idx => idx.Index("comments")
			);
		}

		public async Task BulkIndexComments(List<CommentSearchDocument> documents)
		{
            var bulkRequest = new BulkRequest("comments")
            {
                Operations = new BulkOperationsCollection()
            };

            foreach (var document in documents)
            {
                bulkRequest.Operations.Add(new BulkIndexOperation<CommentSearchDocument>(document)
                {
                    Id = document.Id.ToString()
                });
            }
            var response = await _client.BulkAsync(bulkRequest);
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
