namespace SPA_app.Services.CaptchaS
{
    public interface ICaptchaService
    {
        (string id, byte[] image) Generate();

        Task Validate(string id, string answer);
    }
}
