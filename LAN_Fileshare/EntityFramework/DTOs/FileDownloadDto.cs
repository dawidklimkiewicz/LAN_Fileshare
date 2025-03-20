using System;
using System.ComponentModel.DataAnnotations;

namespace LAN_Fileshare.EntityFramework.DTOs
{
    public class FileDownloadDto
    {
        [Key]
        public Guid Id { get; set; }
        public HostDto Host { get; set; } = null!;
        public string Name { get; set; } = null!;
        public long Size { get; set; }
        public long BytesTransmitted { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
    }
}
