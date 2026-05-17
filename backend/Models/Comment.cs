using SPA_приложение.DTOs;
using SPA_приложение.Helpers;

namespace SPA_приложение.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public Comment? Parent { get; set; }

        public List<Comment> Children { get; set; } = new();
        public List<CommentFile>? Files { get; set; } = new();
        public string UserName { get; set; } = "";

        public string Email { get; set; } = "";

        public string? Homepage { get; set; }

        public string Text { get; set; } = "";

        public int? RootId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Comment(CreateCommentDTO dto)
        {
            UserName = dto.UserName;
            Email = dto.Email;
            Homepage = dto.Homepage;

            Text = HtmlSanitizerHelper.Sanitize(dto.Text);

            ParentId = dto.ParentId;
        }
        public Comment(string userName, string email, string text, DateTime createdAt, int? parentId = null)
        {
            UserName = userName;
            Email = email;

            Text = HtmlSanitizerHelper.Sanitize(text);

            CreatedAt = createdAt;
        }
        private Comment() { }
    }
}
