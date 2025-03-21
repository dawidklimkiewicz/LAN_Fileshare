using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.EntityFramework.DTOs
{
    public class HostDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public PhysicalAddress PhysicalAddress { get; set; } = null!;
        public string DownloadPath { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public bool AutoDownload { get; set; }

        public ICollection<FileUploadDto> FileUploads { get; set; } = null!;
        public ICollection<DownloadTransmissionStatisticsDto> DownloadTransmissionStatistics { get; set; } = null!;
        public ICollection<UploadTransmissionStatisticsDto> UploadTransmissionStatistics { get; set; } = null!;
    }
}
