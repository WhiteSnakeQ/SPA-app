namespace SPA_app.Models.ElasticSearchDocuments
{
    public class CommentSearchDocument
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Text { get; set; } = "";

        public CommentSearchDocument() { }
        public CommentSearchDocument(int id, string userName, string email, string text)
        {
            Id = id;
            UserName = userName;
            Email = email;
            Text = text;
        }
    }
}
