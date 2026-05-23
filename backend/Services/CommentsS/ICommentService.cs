using SPA_app.Enums;
using SPA_приложение.DTOs;



namespace SPA_app.Services.CommentsS
{
    public interface ICommentService
    {
        Task<int> Create(CreateCommentDTO dto);
        Task<CommentsPageDTO> GetCommentsCache(int page, CommentSorting sort, bool desc);
        Task<List<CommentDTO>> GetReplyCache(int comment_id);
    };
}
    
