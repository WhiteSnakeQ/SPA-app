using SPA_app.DTOs.Queries;
using SPA_app.Enums;
using SPA_app.Services.CommentsS;
using SPA_приложение.DTOs;
using SPA_приложение.DTOs.Queries;

namespace SPA_app.GraphQL
{
    public class CommentGQL
    {
        public async Task<CommentsPageDTO>? GetComments(GetCommentsQuery input, [Service] ICommentService comments)
        {
            var result = await comments.GetComments(input.Page, input.Sort, input.Desc);
            return result;
        }

        public async Task<List<CommentDTO>>? GetReplyComments(GetReplyCommentsQueryInput input, [Service] ICommentService comments)
        {
            var result = await comments.GetReply(input.CommentId);
            return result;
        }
    }
}
