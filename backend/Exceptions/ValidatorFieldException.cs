namespace SPA_приложение.Exceptions
{
    public class ValidatorFieldException : Exception
    {
        public int StatusCode { get; }

        public string Field { get; }

        protected ValidatorFieldException(
            string message,
            string field,
            int statusCode = 400)
            : base(message)
        {
            Field = field;

            StatusCode = statusCode;
        }
    }
}
