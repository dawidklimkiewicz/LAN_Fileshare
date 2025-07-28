using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsStore _settingsStore;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _downloadPath;

        [ObservableProperty]
        private bool _autoDownloadDefault;

        public SettingsViewModel(SettingsStore settingsStore)
        {
            _settingsStore = settingsStore;

            Username = settingsStore.Username;
            DownloadPath = settingsStore.DownloadPath;
            AutoDownloadDefault = settingsStore.AutoDownloadDefault;
        }

        partial void OnDownloadPathChanged(string value)
        {
            _settingsStore.DownloadPath = value;
        }

        partial void OnAutoDownloadDefaultChanged(bool value)
        {
            _settingsStore.AutoDownloadDefault = value;
        }

        [RelayCommand]
        private void Close(Window? window)
        {
            window?.Close();
        }

        [RelayCommand]
        private void Save(Window? window)
        {
            bool broadcastChanges = false;
            Username = SanitizeUsername(Username);
            if (_settingsStore.Username != Username)
            {
                _settingsStore.Username = Username;
                broadcastChanges = true;
            }

            _settingsStore.DownloadPath = DownloadPath;

            _settingsStore.Save(broadcastChanges);
            window?.Close();
        }

        [RelayCommand]
        private void RestoreDefaultUsername()
        {
            Username = Environment.UserName;
        }

        [RelayCommand]
        private async Task OpenFolderDialog(Window window)
        {
            FolderDialogService folderDialogService = new(window);
            string? result = await folderDialogService.OpenFolderDialog();
            if (!string.IsNullOrWhiteSpace(result))
            {
                DownloadPath = result;
            }
        }
        private string SanitizeUsername(string input)
        {
            string sanitized = new string(input.Normalize(NormalizationForm.FormC).Where(ch => !char.IsControl(ch)).ToArray());

            return sanitized.Trim();
        }
    }
}
