using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LAN_Fileshare.Stores
{
    public class SettingsStore
    {
        private AppStateStore _appStateStore;
        [JsonIgnore]
        public static readonly string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lan-fileshare");
        [JsonIgnore]
        public static readonly string TemporaryDownloadDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "lan-fileshare", "downloads");
        private static readonly string _configFile = Path.Combine(ConfigDir, "settings.json");

        private string _username = Environment.UserName;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                StrongReferenceMessenger.Default.Send(new NetworkInfoUpdated(_appStateStore));
            }
        }
        public string DownloadPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        public bool AutoDownloadDefault { get; set; } = false;

        public SettingsStore(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
            CreateConfigDir();
        }

        [JsonConstructor]
#pragma warning disable CS8618 // disable null value warning
        public SettingsStore()
#pragma warning restore CS8618 
        {

        }

        public void Load()
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
                        Trace.WriteLine(json);
                        return;
                    }
                }
                else
                {
                    Trace.WriteLine("settings.json file not found - creating a new one");
                }
            }
            catch (Exception ex)
            {
                File.Delete(_configFile);
                Trace.WriteLine($"Failed to load settings {ex}");
            }
            Save();
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
