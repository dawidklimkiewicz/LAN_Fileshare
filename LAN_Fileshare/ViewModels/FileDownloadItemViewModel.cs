﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LAN_Fileshare.ViewModels
{
    public partial class FileDownloadItemViewModel : ObservableObject, IRecipient<BytesTransmittedChangedMessage>, IRecipient<FileStateChangedMessage>, IDisposable
    {
        private readonly FileListingViewModel _parentViewModel;
        private readonly AppStateStore _appStateStore;
        private CancellationTokenSource _transmissionCancellationTokenSource;

        // Variables for estimating remaining time
        private long _lastTransmittedBytes;
        private DateTime _lastBytesUpdateTime;

        public int Progress => (int)((float)BytesTransmitted / Size * 100);
        public TimeSpan EstimatedTimeRemaining => CalculateRemainingTime();
        public double TransmissionSpeed => CalculateDownloadSpeed();
        public FileDownload FileDownload { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public bool IsPaused => FileState == FileState.Paused;
        public bool IsTransmitting => FileState == FileState.Transmitting;
        public bool IsFinished => FileState == FileState.Finished;
        public DateTime TimeCreated { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPaused), [nameof(IsFinished), nameof(IsTransmitting)])]
        private FileState _fileState;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private long _bytesTransmitted;

        [ObservableProperty]
        private bool _isExpanded = false;

        public FileDownloadItemViewModel(FileDownload file, FileListingViewModel parentViewModel, AppStateStore appStateStore)
        {
            _parentViewModel = parentViewModel;
            _appStateStore = appStateStore;

            FileDownload = file;
            Name = file.Name;
            Size = file.Size;
            _fileState = file.State;
            BytesTransmitted = file.BytesTransmitted;
            TimeCreated = file.TimeCreated;

            _transmissionCancellationTokenSource = new();

            StrongReferenceMessenger.Default.Register<BytesTransmittedChangedMessage>(this);
            StrongReferenceMessenger.Default.Register<FileStateChangedMessage>(this);
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }

        [RelayCommand]
        private void RequestFile()
        {
            if (_parentViewModel.SelectedHost != null)
            {
                _transmissionCancellationTokenSource = new();
                FileDataReceiver fileDataReceiver = new(_appStateStore, _parentViewModel.SelectedHost, FileDownload, _transmissionCancellationTokenSource.Token);
                Task listener = fileDataReceiver.SendFileRequest();
                _appStateStore.ActiveFileTransfers.Add(listener);
            }
        }

        [RelayCommand]
        private void PauseDownload()
        {
            if (_parentViewModel.SelectedHost != null && !_transmissionCancellationTokenSource.IsCancellationRequested)
            {
                _transmissionCancellationTokenSource.Cancel();
            }
        }

        [RelayCommand(CanExecute = nameof(RemoveFileCanExecute))]
        private async Task RemoveFile()
        {
            // TODO also remove from temp folder

            FileDownloadItemViewModel? fileDownloadViewModel = _parentViewModel.FileDownloadList.FirstOrDefault(f => f.FileDownload.Id == FileDownload.Id);

            if (fileDownloadViewModel != null)
            {
                _parentViewModel.FileDownloadList.Remove(fileDownloadViewModel);
            }

            NetworkService networkService = new(_appStateStore);
            await networkService.SendRemoveFile(FileDownload.Id, _parentViewModel.SelectedHost!.IPAddress, _appStateStore.PacketListenerPort);
        }

        private bool RemoveFileCanExecute()
        {
            if (FileState == FileState.Transmitting) return false;
            else return true;
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

        public void Receive(BytesTransmittedChangedMessage message)
        {
            if (message.File is FileDownload && message.File.Id == FileDownload.Id)
            {
                BytesTransmitted = message.Value;
            }
        }

        public void Receive(FileStateChangedMessage message)
        {
            if (message.File.Id == FileDownload.Id)
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
