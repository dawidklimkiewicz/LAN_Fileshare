using System;

namespace LAN_Fileshare.Models
{
    public class DownloadTransmissionStatistics
    {
        public string FileType { get; set; } = null!;
        public long Size { get; set; }
        public DateTime Time { get; set; }

        public DownloadTransmissionStatistics(string fileType, long size, DateTime time)
        {
            FileType = fileType;
            Size = size;
            Time = time;
        }
    }
}
