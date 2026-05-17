namespace SPA_приложение.Exceptions
{
    public class InvalidFileException : ValidatorFieldException
    {
        public InvalidFileException(string message, string field) : base(message, field)
        {
        }
        public InvalidFileException(string message) : base(message, "files")
        {
        }
        public InvalidFileException() : base("Invalid captcha", "files") { }
    }
}
