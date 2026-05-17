using SPA_приложение.Exceptions;

namespace SPA_приложение.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidatorFieldException e)
            {
                context.Response.StatusCode = e.StatusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        [e.Field] = [e.Message]
                    }
                });
            }
        }
    }
}
