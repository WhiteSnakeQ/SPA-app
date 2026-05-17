namespace SPA_приложение.DTOs
{
    public class CommentsPageDTO
    {
        public List<CommentDTO> Items { get; set; } = [];
        public bool HasNextPage { get; set; }
    }
}
