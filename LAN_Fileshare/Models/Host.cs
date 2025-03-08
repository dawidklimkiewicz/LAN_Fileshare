using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.Models
{
    public class Host
    {
        public PhysicalAddress PhysicalAddress { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Username { get; set; }
        public string DownloadPath { get; set; }
        public bool IsBlocked { get; set; }
        public FileList<FileUpload> FileUploadList { get; set; }
        public FileList<FileDownload> FileDownloadList { get; set; }


        public Host(PhysicalAddress physicalAddress, IPAddress iPAddress, string username)
        {
            PhysicalAddress = physicalAddress;
            IPAddress = iPAddress;
            Username = username;
            DownloadPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            IsBlocked = false;
            FileUploadList = new();
            FileDownloadList = new();
        }
    }
}
