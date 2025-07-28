using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LAN_Fileshare.Stores
{
    public class SettingsStore
    {
        [JsonIgnore]
        public static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lan-fileshare");
        [JsonIgnore]
        public static readonly string TemporaryDownloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lan-fileshare", "downloads");
        private static readonly string _configFile = Path.Combine(ConfigDir, "settings.json");

        public string Username { get; set; } = "";
        public string DownloadPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        public bool AutoDownloadDefault { get; set; } = false;

        public SettingsStore()
        {
            CreateConfigDir();
            Load();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_configFile))
                {
                    string json = File.ReadAllText(_configFile);
                    SettingsStore? loaded = JsonSerializer.Deserialize<SettingsStore>(json);
                    if (loaded is not null)
                    {
                        Username = loaded.Username;
                        DownloadPath = loaded.DownloadPath;
                        AutoDownloadDefault = loaded.AutoDownloadDefault;
                        return;
                    }

                    File.Delete(_configFile);
                }

                // Create config file if it doesn't exist or is corrupted
                Save();

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to load settings, using defaults: {ex.Message}");
            }
        }

        public void Save()
        {
            try
            {
                JsonSerializerOptions options = new() { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(_configFile, json);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void CreateConfigDir()
        {
            if (!Directory.Exists(ConfigDir))
            {
                DirectoryInfo dir = Directory.CreateDirectory(ConfigDir);
                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }
    }
}
