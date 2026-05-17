using SPA_приложение.Enums;

namespace SPA_приложение.Models
{
    public class CommentFile
    {
        public int Id { get; set; }

        public int CommentId { get; set; }
        public Comment Comment { get; set; } = null!;

        public string FileUrl { get; set; }
        public string FileName { get; set; }

        public FileType FileType { get; set; }

        public long Size { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
        public CommentFile(int commentId, string fileUrl, string fileName, long size, Comment comment, FileType type) 
        { 
            CommentId = commentId;
            Comment = comment;
            FileUrl = fileUrl;
            FileName = fileName;
            FileType = type;
            Size = size;
        }

        private CommentFile() { }
    }
}
