using SPA_приложение.DTOs;

namespace SPA_app.RabbitMQ.Messages
{
	public class CommentCreatedMessage
	{
		public CommentDTO Comment { get; set; }
		
	}
}
