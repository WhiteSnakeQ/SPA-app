using SPA_приложение.Models;

namespace SPA_приложение.DTOs
{
    public class CommentFileDTO
    {
        public int Id { get; set; }

        public string FileUrl { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public CommentFileDTO() { }
        public CommentFileDTO(CommentFile commentFile)
        {
            Id = commentFile.Id;
            FileName = commentFile.FileName;
            FileUrl = commentFile.FileUrl;
            FileType = commentFile.FileType.ToString();
        }
    }
}
