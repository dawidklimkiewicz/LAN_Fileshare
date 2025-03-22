using System;
using System.ComponentModel.DataAnnotations;

namespace LAN_Fileshare.EntityFramework.DTOs
{
    public class FileUploadDto
    {
        [Key]
        public Guid Id { get; set; }
        public HostDto Host { get; set; } = null!;
        public string Path { get; set; } = null!;
        public long BytesTransmitted { get; set; }
        public DateTime TimeCreated { get; set; }
    }
}
