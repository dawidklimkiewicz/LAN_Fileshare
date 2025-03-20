using System;
using System.ComponentModel.DataAnnotations;

namespace LAN_Fileshare.EntityFramework.DTOs
{
    public class DownloadTransmissionStatisticsDto
    {
        [Key]
        public int Id { get; set; }
        public HostDto Host { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public long Size { get; set; }
        public DateTime Time { get; set; }
    }
}
