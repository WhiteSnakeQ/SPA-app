using SPA_app.Enums;
using SPA_приложение.DTOs;



namespace SPA_app.Services.CommentsS
{
    public interface ICommentService
    {
        Task<CommentDTO> Create(CreateCommentDTO dto);
        Task<CommentsPageDTO> GetComments(int page, CommentSorting sort, bool desc);
    };
}
    
