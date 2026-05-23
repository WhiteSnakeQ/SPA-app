using SPA_приложение.DTOs;

namespace SPA_app.RabbitMQ.Messages
{
	public class FileCreatedMessage
	{
		public CommentFileDTO File { get; set; }
        public string FileExt { get; set; } = default!;
    }
}
