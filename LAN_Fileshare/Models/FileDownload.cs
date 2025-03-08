using System;

namespace LAN_Fileshare.Models
{
    public class FileDownload : IFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
        public FileState FileState { get; set; } = FileState.Paused;
        public long BytesTransmitted;

        public FileDownload(Guid fileId, string name, long size)
        {
            Id = fileId;
            Name = name;
            Size = size;
        }
    }
}
