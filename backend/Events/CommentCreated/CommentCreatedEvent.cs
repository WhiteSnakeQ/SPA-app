using SPA_app.Events.Interface;
using SPA_приложение.DTOs;

namespace SPA_app.Events.CommentCreated
{
	public sealed class CommentCreatedEvent : IEvent
	{
		public CommentDTO Comment { get; }

		public CommentCreatedEvent(CommentDTO comment)
		{
			Comment = comment;
		}
	}
}
