namespace SPA_приложение.Exceptions
{
    public class InvalidCaptchaException : ValidatorFieldException
    {
        public InvalidCaptchaException(string message) : base(message, "captchaAnswer")
        {
        }
        public InvalidCaptchaException() : base("Invalid captcha", "captchaAnswer") { }
    }
}
