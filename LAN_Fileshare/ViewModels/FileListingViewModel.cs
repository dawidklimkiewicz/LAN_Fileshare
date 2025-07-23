using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.EntityFramework;
using LAN_Fileshare.EntityFramework.Queries.FileUpload;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileListingViewModel : ObservableObject, IRecipient<SelectedHostChangedMessage>, IRecipient<FileAddedMessage>, IRecipient<FileRemovedMessage>, IDisposable
    {
        private readonly AppStateStore _appStateStore;
        private readonly FileDialogService _fileDialogService;
        private readonly MainDbContextFactory _mainDbContextFactory;

        public Host? SelectedHost { get; set; } = null;
        public bool IsAnyHostSelected => SelectedHost != null;
        public ObservableCollection<FileUploadItemViewModel> FileUploadList { get; } = new();
        public ObservableCollection<FileDownloadItemViewModel> FileDownloadList { get; } = new();
        public ObservableCollection<FileUploadItemViewModel> FilteredFileUploadList { get; } = new();
        public ObservableCollection<FileDownloadItemViewModel> FilteredFileDownloadList { get; } = new();

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private string _selectedHostName;

        [ObservableProperty]
        private IPAddress _selectedHostIp;
        
        partial void OnSearchTextChanged(string value) => SearchFiles();

        public FileListingViewModel(AppStateStore appStateStore, FileDialogService fileDialogService, MainDbContextFactory mainDbContextFactory)
        {
            _appStateStore = appStateStore;
            _fileDialogService = fileDialogService;
            _mainDbContextFactory = mainDbContextFactory;

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
                    FileUploadItemViewModel viewModel = new FileUploadItemViewModel(file, this, _appStateStore);
                    FileUploadList.Add(viewModel);
                }

                foreach (FileDownload file in SelectedHost.FileDownloadList.GetAll())
                {
                    FileDownloadItemViewModel viewModel = new FileDownloadItemViewModel(file, this, _appStateStore);
                    FileDownloadList.Add(viewModel);
                }

                SearchFiles();
            }
        }

        private void SearchFiles()
        {
            FilteredFileDownloadList.Clear();
            FilteredFileUploadList.Clear();

            if (SearchText == "")
            {
                foreach (FileUploadItemViewModel file in FileUploadList)
                {
                    FilteredFileUploadList.Add(file);
                }
                foreach (FileDownloadItemViewModel file in FileDownloadList)
                {
                    FilteredFileDownloadList.Add(file);
                }
            }
            else
            {
                List<FileUploadItemViewModel> filteredUploads = FileUploadList.Where(file => file.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                List<FileDownloadItemViewModel> filteredDownloads = FileDownloadList.Where(file => file.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (FileUploadItemViewModel file in filteredUploads)
                {
                    FilteredFileUploadList.Add(file);
                }
                foreach (FileDownloadItemViewModel file in filteredDownloads)
                {
                    FilteredFileDownloadList.Add(file);
                }
            }
        }

        [RelayCommand]
        private async Task ClearFinishedUploads()
        {
            List<FileUploadItemViewModel> finishedUploads = FileUploadList.Where(file => file.IsFinished).ToList();
            foreach (FileUploadItemViewModel file in finishedUploads)
            {
                await file.RemoveFile();
            }
        }

        [RelayCommand]
        private async Task ClearFinishedDownloads()
        {
            List<FileDownloadItemViewModel> finishedDownloads = FileDownloadList.Where(file => file.IsFinished).ToList();
            foreach (FileDownloadItemViewModel file in finishedDownloads)
            {
                await file.RemoveFile();
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

                try
                {
                    NetworkService networkService = new(_appStateStore);
                    await networkService.SendFileInformation(filesList, SelectedHost.IPAddress, _appStateStore.PacketListenerPort);

                    CreateFileUpload createFileUpload = new(_mainDbContextFactory);
                    await createFileUpload.Execute(SelectedHost, filesList);
                }

                catch (Exception ex)
                {
                    Trace.WriteLine($"Error uploading files: {ex}");
                }
            }
        }

        public void Receive(SelectedHostChangedMessage message)
        {
            SelectedHost = message.Value?.Host;
            SelectedHostName = _appStateStore.SelectedHost?.Username ?? "";
            SelectedHostIp = _appStateStore.SelectedHost?.IPAddress ?? IPAddress.None;
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
                FilteredFileUploadList.Add(viewModel);
            }
            else if (message.File is FileDownload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileDownloadItemViewModel viewModel = new FileDownloadItemViewModel((FileDownload)message.File, this, _appStateStore);
                FileDownloadList.Add(viewModel);
                FilteredFileDownloadList.Add(viewModel);
            }
        }

        public async void Receive(FileRemovedMessage message)
        {
            if (SelectedHost == null) return;

            if (message.File is FileUpload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileUploadItemViewModel? viewModel = FileUploadList.FirstOrDefault(file => file.FileUpload.Id.Equals(message.File.Id));
                if (viewModel == null) return;
                FileUploadList.Remove(viewModel);
                FilteredFileUploadList.Remove(viewModel);

                try
                {
                    DeleteFileUpload deleteFileUpload = new(_mainDbContextFactory);
                    await deleteFileUpload.Execute(viewModel.FileUpload.Id);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error deleting file in db: {ex}");
                }
            }
            else if (message.File is FileDownload && SelectedHost.PhysicalAddress.Equals(message.Host.PhysicalAddress))
            {
                FileDownloadItemViewModel? viewModel = FileDownloadList.FirstOrDefault(file => file.FileDownload.Id.Equals(message.File.Id));
                if (viewModel == null) return;
                FileDownloadList.Remove(viewModel);
                FilteredFileDownloadList.Remove(viewModel);
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
