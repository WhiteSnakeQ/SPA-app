using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using SPA_app.Models.ElasticSearchDocuments;
using SPA_приложение.Data;

namespace SPA_app.Services.ElasticIndexIntialazer
{
    public class ElasticSearchInitializer
    {
        private readonly ElasticsearchClient _client;
        private readonly AppDbContext _db;

        public ElasticSearchInitializer(ElasticsearchClient client, AppDbContext db)
        {
            _client = client;
            _db = db;
        }

        public async Task Initialize()
        {
            var exists = await _client.Indices.ExistsAsync("comments");

            if (exists.Exists)
                return;

            await CreateIndex();

            var comments = await _db.Comments.ToListAsync();
            var documents = comments.Select(c => new CommentSearchDocument(c.Id, c.UserName, c.Email, c.Text));

            await _client.BulkAsync(b => b
                .Index("comments")
                .IndexMany(documents)
            );
        }

        private async Task CreateIndex()
        {
            await _client.Indices.CreateAsync(
                "comments",
                c => c
                    .Mappings(m => m
                        .Properties<CommentSearchDocument>(p => p
                            .Text(t => t.Text, x => x.Analyzer("english"))
                            .Text(t => t.UserName)
                            .Text(t => t.Email)
                        )));
        }
    }
}
