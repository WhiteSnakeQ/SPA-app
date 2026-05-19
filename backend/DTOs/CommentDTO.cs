using SPA_приложение.Models;

namespace SPA_приложение.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentFileDTO> Files { get; set; } = [];

        public List<CommentDTO> Children { get; set; } = [];

        public CommentDTO() { }
        public CommentDTO(Comment comment)
        {
            Id = comment.Id;
            ParentId = comment.ParentId;
            UserName = comment.UserName;
            Email = comment.Email;
            Text = comment.Text;
            CreatedAt = comment.CreatedAt;

            if (comment.Files != null)
            {
                Files = comment.Files
                    .Select(x => new CommentFileDTO(x))
                    .ToList();
            }

            if (comment.Children != null)
            {
                Children = comment.Children
                    .Select(x => new CommentDTO(x))
                    .ToList();
            }
        }

        public CommentDTO(Comment comment, List<CommentFileDTO> files, List<CommentDTO> children)
        {
            Id = comment.Id;
            ParentId = comment.ParentId;
            UserName = comment.UserName;
            Email = comment.Email;
            Text = comment.Text;
            CreatedAt = comment.CreatedAt;

            Files = files;
            Children = children;
        }
    }
}
