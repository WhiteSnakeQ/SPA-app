using HotChocolate.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SPA_app.Constants;
using SPA_app.Services.CacheS;
using SPA_приложение.Enums;
using System;

namespace SPA_app.Services.ImageS;

public sealed class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task ResizeImage(string fullPath, string fileExt)
    {
        var physicalPath = System.IO.Path.Combine(_env.WebRootPath, fullPath.TrimStart('/'));
        using var image = await Image.LoadAsync(physicalPath);

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

        var tempPath = physicalPath + ".tmp";

        switch (fileExt)
        {
            case ".jpg":
                await image.SaveAsJpegAsync(tempPath);
                break;

            case ".png":
                await image.SaveAsPngAsync(tempPath);
                break;

            case ".gif":
                await image.SaveAsGifAsync(tempPath);
                break;
        }

        File.Delete(physicalPath);
        File.Move(tempPath, physicalPath);
    }
}