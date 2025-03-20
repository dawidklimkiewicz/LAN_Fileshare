namespace LAN_Fileshare.Models
{
    public class Settings
    {
        public string Username { get; set; }
        public string DefaultDownloadPath { get; set; }
        public bool AutoDownloadDefault { get; set; }
        public bool CopyUploadedFileToWorkspace { get; set; }
        public long CopyUploadedFileToWorkspaceMaxSize { get; set; }
    }
}
