using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SPA_приложение.Constants;
using SPA_приложение.Exceptions;
using System.Security.Cryptography;

namespace SPA_приложение.Services
{
    public interface ICaptchaService
    {
        (string id, byte[] image) Generate();

        Task Validate(string id, string answer);
    }

    public class CaptchaService : ICaptchaService
    {
        private readonly IMemoryCache _cache;
        public CaptchaService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public (string, byte[]) Generate()
        {
            var text = RandomText();

            var id = Guid.NewGuid().ToString();

            _cache.Set(
                id,
                text,
                TimeSpan.FromMinutes(5));

            var image = GenerateImage(text);

            return (id, image);
        }

        public Task Validate(string id, string answer)
        {
            
            if (!_cache.TryGetValue(id, out string? expected))
                throw new InvalidCaptchaException("Captcha expired");

            _cache.Remove(id);

            if (!string.Equals(expected, answer, StringComparison.OrdinalIgnoreCase))
                throw new InvalidCaptchaException("Invalid captcha");

            return Task.CompletedTask;
        }

        private string RandomText(int length = 5)
        {
            const string chars = CaptchaConstants.captchaChars;

            var random = new Random();

            return new string(Enumerable.Range(0, length)
                    .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                    .ToArray());
        }

        private byte[] GenerateImage(string text)
        {
            int SizeX = CaptchaConstants.Width;
            int SizeY = CaptchaConstants.Height;

            using var image = new Image<Rgba32>(SizeX, SizeY);

            var random = new Random();

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                var family = SystemFonts.Collection.Families.First();

                var font = family.CreateFont(
                        38,
                        FontStyle.Bold);

                for (int i = 0; i < CaptchaConstants.Lines.LineCount; i++)
                {
                    ctx.DrawLine(
                        Color.Gray,
                        random.Next(1, 3),

                        new PointF(
                            random.Next(0, CaptchaConstants.Lines.fPointX),
                            random.Next(CaptchaConstants.Lines.PointYMin, CaptchaConstants.Lines.PointYMax)),

                        new PointF(
                            random.Next(CaptchaConstants.Lines.lPointX, SizeX),
                            random.Next(CaptchaConstants.Lines.PointYMin, CaptchaConstants.Lines.PointYMax)));
                }

                for (int i = 0; i < text.Length; i++)
                {
                    int startX = CaptchaConstants.Letters.StartX;
                    int indentX = CaptchaConstants.Letters.indentX;
                    int x =
                        startX + i * indentX;

                    int MinY = CaptchaConstants.Letters.MinY;
                    int MaxY = CaptchaConstants.Letters.MaxY;
                    int y =
                        random.Next(MinY, MaxY);

                    int angleMin = CaptchaConstants.Letters.angleMin;
                    int angleMax = CaptchaConstants.Letters.angleMax;
                    int angle =
                        random.Next(angleMin, angleMax);

                    var character =
                        text[i].ToString();

                    ctx.DrawText(
                        character,
                        font,
                        Color.Black,
                        new PointF(x, y));

                    ctx.Rotate(angle);
                }

                for (int i = 0; i < 300; i++)
                {
                    image[
                        random.Next(image.Width),
                        random.Next(image.Height)
                    ] = Color.LightGray;
                }
            });

            using var stream = new MemoryStream();

            image.SaveAsPng(stream);

            return stream.ToArray();
        }
    }
    
}
