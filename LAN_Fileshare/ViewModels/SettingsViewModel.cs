using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LAN_Fileshare.Stores;
using System;

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
            _settingsStore.Username = Username;
            _settingsStore.Save();
            window?.Close();
        }

        [RelayCommand]
        private void RestoreDefaultUsername()
        {
            Username = Environment.UserName;
        }
    }
}
