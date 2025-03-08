﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileListingViewModel : ObservableObject, IRecipient<SelectedHostChangedMessage>, IRecipient<FileAddedMessage>, IDisposable
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
        }

        private void GetFileViewModels()
        {
            FileUploadList.Clear();
            FileDownloadList.Clear();

            if (SelectedHost != null)
            {
                foreach (FileUpload file in SelectedHost.FileUploadList.GetAll())
                {
                    FileUploadList.Add(new FileUploadItemViewModel(file));
                }

                foreach (FileDownload file in SelectedHost.FileDownloadList.GetAll())
                {
                    FileDownloadList.Add(new FileDownloadItemViewModel(file));
                }
            }
        }

        [RelayCommand]
        private async Task OpenFileDialog()
        {
            string[]? files = await _fileDialogService.OpenFileDialogAsync();

            if (files != null && SelectedHost != null)
            {
                IEnumerable<FileUpload> newFiles = files.Select(file => new FileUpload(file));
                SelectedHost.FileUploadList.AddRange(newFiles);
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
            if (message.File is FileUpload)
            {
                FileUploadItemViewModel viewModel = new FileUploadItemViewModel((FileUpload)message.File);
                FileUploadList.Add(viewModel);
            }
            else if (message.File is FileDownload)
            {
                FileDownloadItemViewModel viewModel = new FileDownloadItemViewModel((FileDownload)message.File);
                FileDownloadList.Add(viewModel);
            }
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<SelectedHostChangedMessage>(this);
            StrongReferenceMessenger.Default.Unregister<FileAddedMessage>(this);
        }
    }
}
