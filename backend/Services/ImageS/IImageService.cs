using SPA_приложение.Enums;

namespace SPA_app.Services.ImageS
{
    public interface IImageService
    {
        Task ResizeImage(string fullPath, FileType fileType, string FileExt);
    }
}
