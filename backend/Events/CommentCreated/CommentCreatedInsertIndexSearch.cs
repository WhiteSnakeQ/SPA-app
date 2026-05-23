using SPA_app.Events.Interface;
using SPA_app.Services.ElasticSearch;

namespace SPA_app.Events.CommentCreated
{
	public class CommentCreatedInsertIndexSearch : IEventHandler<CommentCreatedEvent>
	{
		private readonly ICommentSearchService _commentSearchService;

		public CommentCreatedInsertIndexSearch(ICommentSearchService commentSearchService)
		{
			_commentSearchService = commentSearchService;
		}

		public async Task Handle(CommentCreatedEvent @event)
		{
            await _commentSearchService.IndexComment(@event.Comment);

        }
	}
}
