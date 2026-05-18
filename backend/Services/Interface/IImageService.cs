using SPA_приложение.Enums;

namespace SPA_app.Services.Interface
{
    public interface IImageService
    {
        Task ResizeImage(string fullPath, FileType fileType, string FileExt);
    }
}
