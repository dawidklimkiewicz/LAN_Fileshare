using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileListingViewModel : ObservableObject, IRecipient<SelectedHostChangedMessage>, IRecipient<FileAddedMessage>, IRecipient<FileRemovedMessage>, IDisposable
    {
        private readonly AppStateStore _appStateStore;
        private readonly FileDialogService _fileDialogService;

        public Host? SelectedHost { get; set; } = null;
        public bool IsAnyHostSelected => SelectedHost != null;
        public ObservableCollection<FileUploadItemViewModel> FileUploadList { get; } = new();
        public ObservableCollection<FileDownloadItemViewModel> FileDownloadList { get; } = new();

        public FileListingViewModel(AppStateStore appStateStore, FileDialogService fileDialogService)
        {
            _appStateStore = appStateStore;
            _fileDialogService = fileDialogService;

            StrongReferenceMessenger.Default.Register<SelectedHostChangedMessage>(this);
            StrongReferenceMessenger.Default.Register<FileAddedMessage>(this);
            StrongReferenceMessenger.Default.Register<FileRemovedMessage>(this);
        }

        private void GetFileViewModels()
        {
            FileUploadList.Clear();
            FileDownloadList.Clear();

            if (SelectedHost != null)
            {
                foreach (FileUpload file in SelectedHost.FileUploadList.GetAll())
                {
                    FileUploadList.Add(new FileUploadItemViewModel(file, this, _appStateStore));
                }

                foreach (FileDownload file in SelectedHost.FileDownloadList.GetAll())
                {
                    FileDownloadList.Add(new FileDownloadItemViewModel(file, this, _appStateStore));
                }
            }
        }

        [RelayCommand]
        private async Task OpenFileDialog()
        {
            string[]? filePaths = await _fileDialogService.OpenFileDialogAsync();
            await UploadFiles(filePaths);
        }

        [RelayCommand]
        private async Task DropFiles(string[]? filePaths)
        {
            await UploadFiles(filePaths);
        }

        private async Task UploadFiles(string[]? files)
        {
            if (SelectedHost != null && files != null)
            {
                if (files.Length == 0 || files.Length > 100) return;
                List<FileUpload> filesList = new();

                foreach (string path in files)
                {
                    if (SelectedHost.FileUploadList.GetAll().Any(f => f.Path == path) || new FileInfo(path).Length == 0) continue;
                    else
                    {
                        FileUpload newFile = new(path);
                        filesList.Add(newFile);
                    }
                }
                SelectedHost.FileUploadList.AddRange(filesList);

                NetworkService networkService = new(_appStateStore);
                await networkService.SendFileInformation(filesList, SelectedHost.IPAddress, _appStateStore.PacketListenerPort);
            }
        }

        public void Receive(SelectedHostChangedMessage message)
        {
            SelectedHost = message.Value?.Host;
            OnPropertyChanged(nameof(IsAnyHostSelected));
            GetFileViewModels();
        }

        public void Receive(FileAddedMessage message)
        {
            if (SelectedHost == null) return;

            if (message.File is FileUpload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileUploadItemViewModel viewModel = new FileUploadItemViewModel((FileUpload)message.File, this, _appStateStore);
                FileUploadList.Add(viewModel);
            }
            else if (message.File is FileDownload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileDownloadItemViewModel viewModel = new FileDownloadItemViewModel((FileDownload)message.File, this, _appStateStore);
                FileDownloadList.Add(viewModel);
            }
        }

        public void Receive(FileRemovedMessage message)
        {
            if (SelectedHost == null) return;

            if (message.File is FileUpload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileUploadItemViewModel? viewModel = FileUploadList.FirstOrDefault(file => file.FileUpload.Id.Equals(message.File.Id));
                if (viewModel == null) return;
                FileUploadList.Remove(viewModel);
            }
            else if (message.File is FileDownload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileDownloadItemViewModel? viewModel = FileDownloadList.FirstOrDefault(file => file.FileDownload.Id.Equals(message.File.Id));
                if (viewModel == null) return;
                FileDownloadList.Remove(viewModel);
            }
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<SelectedHostChangedMessage>(this);
            StrongReferenceMessenger.Default.Unregister<FileAddedMessage>(this);
            StrongReferenceMessenger.Default.Unregister<FileRemovedMessage>(this);
        }
    }
}
