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

    public async Task ResizeImage(string fullPath, FileType fileType, string FileExt)
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

        switch (FileExt)
        {
            case ".jpg":
                await image.SaveAsJpegAsync(physicalPath);
                break;

            case ".png":
                await image.SaveAsPngAsync(physicalPath);
                break;

            case ".gif":
                await image.SaveAsGifAsync(physicalPath);
                break;
        }
    }
}