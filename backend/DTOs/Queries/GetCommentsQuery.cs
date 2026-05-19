using SPA_app.Enums;

namespace SPA_приложение.DTOs.Queries
{    public class GetCommentsQuery
    {
        public int Page { get; set; } = 0;

        public CommentSorting Sort { get; set; } = CommentSorting.CreatedAt;

        public bool Desc { get; set; } = true;
    }
}
