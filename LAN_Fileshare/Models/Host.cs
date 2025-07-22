using LAN_Fileshare.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LAN_Fileshare.Models
{
    public class Host
    {
        public PhysicalAddress PhysicalAddress { get; set; }
        public IPAddress IPAddress { get; set; }
        public string Username { get; set; }
        public string DownloadPath { get; set; }
        public bool IsBlocked { get; set; }
        public bool AutoDownload { get; set; }
        public FileList<FileUpload> FileUploadList { get; set; }
        public FileList<FileDownload> FileDownloadList { get; set; }
        public HostConnection Connection { get; set; }


        public Host(PhysicalAddress physicalAddress, IPAddress iPAddress, string username, TcpClient client)
        {
            PhysicalAddress = physicalAddress;
            IPAddress = iPAddress;
            Username = username;
            DownloadPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            IsBlocked = false;
            FileUploadList = new(this);
            FileDownloadList = new(this);
            Connection = new(client);
        }

        public Host(PhysicalAddress physicalAddress, IPAddress ipAddress, string username, string downloadPath, bool isBlocked, bool autoDownload, List<FileUpload> fileUploads, TcpClient client)
        {
            PhysicalAddress = physicalAddress;
            IPAddress = ipAddress;
            Username = username;
            DownloadPath = downloadPath;
            IsBlocked = isBlocked;
            AutoDownload = autoDownload;
            FileUploadList = new(this, fileUploads);
            FileDownloadList = new(this);
            Connection = new(client);
        }
    }
}
