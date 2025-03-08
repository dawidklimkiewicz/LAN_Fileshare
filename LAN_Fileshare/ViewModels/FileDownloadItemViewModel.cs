using LAN_Fileshare.Models;

namespace LAN_Fileshare.ViewModels
{
    public class FileDownloadItemViewModel
    {
        public FileDownload FileDownload { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public FileState FileState { get; set; }

        public FileDownloadItemViewModel(FileDownload file)
        {
            FileDownload = file;
            Name = file.Name;
            Size = file.Size;
            FileState = file.FileState;
        }
    }
}
