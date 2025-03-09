using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileUploadItemViewModel : ObservableObject
    {
        private readonly FileListingViewModel _parentViewModel;
        private readonly AppStateStore _appStateStore;

        // Variables for estimating remaining time
        private long _lastTransmittedBytes;
        private DateTime _lastBytesUpdateTime;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private long _bytesTransmitted;
        public int Progress => (int)((float)BytesTransmitted / Size * 100);
        public TimeSpan EstimatedTimeRemaining => CalculateRemainingTime();
        public double TransmissionSpeed => CalculateDownloadSpeed();
        public FileUpload FileUpload { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public FileState FileState { get; set; }
        public bool IsPaused => FileState == FileState.Paused;
        public bool IsTransmitting => FileState == FileState.Transmitting;
        public Guid Id { get; set; }


        public FileUploadItemViewModel(FileUpload file, FileListingViewModel parentViewModel, AppStateStore appStateStore)
        {
            _parentViewModel = parentViewModel;
            _appStateStore = appStateStore;

            Id = file.Id;
            FileUpload = file;
            Name = file.Name;
            Size = file.Size;
            FileState = file.FileState;
            BytesTransmitted = file.BytesTransmitted;
        }

        [RelayCommand]
        private async Task RemoveFile(Guid fileId)
        {
            FileUploadItemViewModel? fileUploadViewModel = _parentViewModel.FileUploadList.FirstOrDefault(f => f.Id == fileId);
            FileDownloadItemViewModel? fileDownloadViewModel = _parentViewModel.FileDownloadList.FirstOrDefault(f => f.Id == fileId);

            if (fileUploadViewModel != null)
            {
                _parentViewModel.FileUploadList.Remove(fileUploadViewModel);
            }

            if (fileDownloadViewModel != null)
            {
                _parentViewModel.FileDownloadList.Remove(fileDownloadViewModel);
            }

            NetworkService networkService = new(_appStateStore);
            await networkService.SendRemoveFile(fileId, _parentViewModel.SelectedHost!.IPAddress, _appStateStore.PacketListenerPort);
        }

        private TimeSpan CalculateRemainingTime()
        {
            long bytesRemaining = Size - BytesTransmitted;
            if (TransmissionSpeed > 1000)
            {
                return TimeSpan.FromSeconds(bytesRemaining / TransmissionSpeed);
            }
            else
            {
                return TimeSpan.Zero;
            }
        }

        private double CalculateDownloadSpeed()
        {
            long transmittedSinceLastUpdate = BytesTransmitted - _lastTransmittedBytes;
            double secondsElapsed = (DateTime.Now - _lastBytesUpdateTime).TotalSeconds;

            _lastTransmittedBytes = BytesTransmitted;
            _lastBytesUpdateTime = DateTime.Now;

            return transmittedSinceLastUpdate / secondsElapsed;
        }
    }
}
