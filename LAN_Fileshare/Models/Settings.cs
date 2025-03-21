namespace LAN_Fileshare.Models
{
    public class Settings
    {
        public string Username { get; set; }
        public string DefaultDownloadPath { get; set; }
        public bool AutoDownloadDefault { get; set; } = false;
        public bool CopyUploadedFileToWorkspace { get; set; } = false;
        public long CopyUploadedFileToWorkspaceMaxSize { get; set; } = 1024 * 1024 * 100;

        public Settings(string username, string defaultDownloadPath)
        {
            Username = username;
            DefaultDownloadPath = defaultDownloadPath;
        }

        public Settings(string username, string defaultDownloadPath, bool autoDownloadDefault, bool copyUploadedFileToWorkspace, long copyUploadedFileToWorkspaceMaxSize)
        {
            Username = username;
            DefaultDownloadPath = defaultDownloadPath;
            AutoDownloadDefault = autoDownloadDefault;
            CopyUploadedFileToWorkspace = copyUploadedFileToWorkspace;
            CopyUploadedFileToWorkspaceMaxSize = copyUploadedFileToWorkspaceMaxSize;
        }
    }
}
