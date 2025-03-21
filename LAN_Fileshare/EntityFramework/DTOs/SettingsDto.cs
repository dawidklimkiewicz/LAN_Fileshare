using System.ComponentModel.DataAnnotations;

namespace LAN_Fileshare.EntityFramework.DTOs
{
    public class SettingsDto
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string DefaultDownloadPath { get; set; } = null!;
        public bool AutoDownloadDefault { get; set; }
        public bool CopyUploadedFileToWorkspace { get; set; }
        public long CopyUploadedFileToWorkspaceMaxSize { get; set; }
    }
}
