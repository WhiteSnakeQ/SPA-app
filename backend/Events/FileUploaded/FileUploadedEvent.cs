using SPA_app.Events.Interface;
using SPA_приложение.Enums;

namespace SPA_app.Events.FileUploaded
{
    public class FileUploadedEvent : IEvent
    {
        public string FilePath;
        public string ext;
        public FileType FileType;

        public FileUploadedEvent(string filePath, FileType fileType, string extension )
        {
            FilePath = filePath;
            FileType = fileType;
            ext = extension;
        }
    }
}
