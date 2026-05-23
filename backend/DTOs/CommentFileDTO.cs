using SPA_приложение.Enums;
using SPA_приложение.Models;

namespace SPA_приложение.DTOs
{
    public class CommentFileDTO
    {
        public int Id { get; set; }

        public string FileUrl { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }
        public int CommentId { get; set; } = 0;
        public long Size { get; set; } = 0;
        public FileType Type { get; set; } = Enums.FileType.Text;

        public CommentFileDTO() { }
        public CommentFileDTO(CommentFile commentFile, int commentId, long size, FileType type)
        {
            Id = commentFile.Id;
            FileName = commentFile.FileName;
            FileUrl = commentFile.FileUrl;
            FileType = commentFile.FileType.ToString();
            CommentId = commentId;
            Size = size;
            Type = type;
        }
        public CommentFileDTO(CommentFile commentFile)
        {
            Id = commentFile.Id;
            FileName = commentFile.FileName;
            FileUrl = commentFile.FileUrl;
            FileType = commentFile.FileType.ToString();
        }
    }
}
