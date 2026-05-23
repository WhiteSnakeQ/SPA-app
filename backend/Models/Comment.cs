using SPA_приложение.DTOs;
using SPA_приложение.Helpers;

namespace SPA_приложение.Models
{
	public class Comment
	{
		public int Id { get; set; }

		public int? ParentId { get; set; }
		public Guid? RequestId { get; set; }

		public Comment? Parent { get; set; }

		public List<Comment> Children { get; set; } = new();
		public List<CommentFile>? Files { get; set; } = new();
		public string UserName { get; set; } = "";

		public string Email { get; set; } = "";

		public string? Homepage { get; set; }

		public string Text { get; set; } = "";

		public Guid RootId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Comment(string userName, string email, string? homePage, Guid requestId, Guid rootId, string text, int? parentId)
		{
			RootId = rootId;
            UserName = userName;
			Email = email;
			Homepage = homePage;
			RequestId = requestId;
			Text = text;

			ParentId = parentId;
		}
		public Comment(string userName, string email, string text, DateTime createdAt, int? parentId = null)
		{
			UserName = userName;
			Email = email;

			Text = text;

			CreatedAt = createdAt;
			ParentId = parentId;
		}
		private Comment() { }
	}
}
