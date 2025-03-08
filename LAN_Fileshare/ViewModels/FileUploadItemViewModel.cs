using CommunityToolkit.Mvvm.ComponentModel;
using LAN_Fileshare.Models;
using System;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileUploadItemViewModel : ObservableObject
    {
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


        public FileUploadItemViewModel(FileUpload file)
        {
            FileUpload = file;
            Name = file.Name;
            Size = file.Size;
            FileState = file.FileState;
            BytesTransmitted = file.BytesTransmitted;
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
