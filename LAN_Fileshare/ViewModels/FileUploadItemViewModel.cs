using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileUploadItemViewModel : ObservableObject, IRecipient<BytesTransmittedChangedMessage>, IRecipient<FileStateChangedMessage>, IDisposable
    {
        private readonly FileListingViewModel _parentViewModel;
        private readonly AppStateStore _appStateStore;

        // Variables for estimating remaining time
        private long _lastTransmittedBytes;
        private DateTime _lastBytesUpdateTime;

        public int Progress => (int)((float)BytesTransmitted / Size * 100);
        public FileUpload FileUpload { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public bool IsPaused => FileState == FileState.Paused;
        public bool IsTransmitting => FileState == FileState.Transmitting;
        public bool IsFinished => FileState == FileState.Finished;
        public DateTime TimeCreated { get; set; }

        [ObservableProperty]
        public TimeSpan _estimatedTimeRemaining;

        [ObservableProperty]
        private double _transmissionSpeed;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private long _bytesTransmitted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPaused), nameof(IsFinished), nameof(IsTransmitting))]
        private FileState _fileState;

        [ObservableProperty]
        private bool _isExpanded = false;

        public FileUploadItemViewModel(FileUpload file, FileListingViewModel parentViewModel, AppStateStore appStateStore)
        {
            _parentViewModel = parentViewModel;
            _appStateStore = appStateStore;

            FileUpload = file;
            Name = file.Name;
            Path = file.Path;
            Size = file.Size;
            FileState = file.State;
            BytesTransmitted = file.BytesTransmitted;
            TimeCreated = file.TimeCreated;

            StrongReferenceMessenger.Default.Register<BytesTransmittedChangedMessage>(this);
            StrongReferenceMessenger.Default.Register<FileStateChangedMessage>(this);
        }

        [RelayCommand]
        private void OpenFolder()
        {
            Process.Start("explorer.exe", @$"{System.IO.Path.GetDirectoryName(Path)}");
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }

        [RelayCommand(CanExecute = nameof(RemoveFileCanExecute))]
        public async Task RemoveFile()
        {
            FileUploadItemViewModel? fileUploadViewModel = _parentViewModel.FileUploadList.FirstOrDefault(f => f.FileUpload.Id == FileUpload.Id);

            if (fileUploadViewModel != null)
            {
                _parentViewModel.SelectedHost?.FileUploadList.Remove(FileUpload);
            }

            NetworkService networkService = new(_appStateStore);
            await networkService.SendRemoveFile(FileUpload.Id, _parentViewModel.SelectedHost!.IPAddress, _appStateStore.PacketListenerPort);
        }

        private bool RemoveFileCanExecute()
        {
            if (FileState == FileState.Transmitting) return false;
            else return true;
        }

        private void CalculateRemainingTime()
        {
            long bytesRemaining = Size - BytesTransmitted;
            if (TransmissionSpeed > 1000)
            {
                Trace.WriteLine($"Bytes remaining: {bytesRemaining} - Speed: {TransmissionSpeed} - ET: {TimeSpan.FromSeconds(bytesRemaining / TransmissionSpeed).ToString()}");
                EstimatedTimeRemaining = TimeSpan.FromSeconds(bytesRemaining / TransmissionSpeed);
            }
            else
            {
                EstimatedTimeRemaining = TimeSpan.Zero;
            }
        }

        private void CalculateDownloadSpeed()
        {
            long transmittedSinceLastUpdate = BytesTransmitted - _lastTransmittedBytes;
            double secondsElapsed = (DateTime.Now - _lastBytesUpdateTime).TotalSeconds;

            _lastTransmittedBytes = BytesTransmitted;
            _lastBytesUpdateTime = DateTime.Now;

            TransmissionSpeed = transmittedSinceLastUpdate / secondsElapsed;
        }

        public void Receive(BytesTransmittedChangedMessage message)
        {
            if (message.File is FileUpload && message.File.Id == FileUpload.Id)
            {
                BytesTransmitted = message.Value;
                CalculateDownloadSpeed();
                CalculateRemainingTime();
            }
        }

        public void Receive(FileStateChangedMessage message)
        {
            if (message.File.Id == FileUpload.Id)
            {
                FileState = message.Value;
            }
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<BytesTransmittedChangedMessage>(this);
            StrongReferenceMessenger.Default.Unregister<FileStateChangedMessage>(this);
        }
    }
}
