namespace SPA_app.RabbitMQ.Messages
{
    public class ImageResizeMessage
    {
        public string FullPath { get; set; } = default!;
        public string FileExt { get; set; } = default!;
    }
}
