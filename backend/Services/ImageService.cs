using SixLabors.ImageSharp;

using SixLabors.ImageSharp.Processing;
using SPA_app.Constants;
using SPA_app.Services.Interface;
using SPA_приложение.Enums;

namespace SPA_приложение.Services;

public sealed class ImageService : IImageService
{
    public async Task ResizeImage(string fullPath, FileType fileType, string FileExt)
    {
        using var image = await Image.LoadAsync(fullPath);

        int width = ImageConsts.Width;
        int height = ImageConsts.Height;

        if (image.Width <= width && image.Height <= height)
            return;
        
        image.Mutate(x =>
            x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, height)
            }));

        switch (FileExt)
        {
            case ".jpg":
                await image.SaveAsJpegAsync(
                    fullPath);
                break;

            case ".png":
                await image.SaveAsPngAsync(
                    fullPath);
                break;

            case ".gif":
                await image.SaveAsGifAsync(
                    fullPath);
                break;
        }
    }
}