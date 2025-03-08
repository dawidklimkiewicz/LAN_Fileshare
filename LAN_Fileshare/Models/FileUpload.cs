using System;
using System.IO;

namespace LAN_Fileshare.Models
{
    public class FileUpload : IFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
        public FileState FileState { get; set; } = FileState.Paused;
        public long BytesTransmitted;

        public FileUpload(string filePath)
        {
            Id = Guid.NewGuid();
            FileInfo fileInfo = new FileInfo(filePath);
            Path = filePath;
            Name = fileInfo.Name;
            Size = fileInfo.Length;
            TimeCreated = DateTime.Now;
            BytesTransmitted = 0;
        }
    }
}
