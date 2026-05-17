namespace SPA_приложение.DTOs.Queries
{    public class GetCommentsQuery
    {
        public int Page { get; set; } = 0;

        public string Sort { get; set; } = "date";

        public bool Desc { get; set; } = true;
    }
}
