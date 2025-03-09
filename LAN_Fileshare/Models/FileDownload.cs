using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using System;
using System.Threading;

namespace LAN_Fileshare.Models
{
    public class FileDownload : IFile
    {
        private Timer? _bytesTransmittedNotificationTimer;
        private object _bytesTransmittedLock = new();
        private long _bytesTransmittedLastValue;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeFinished { get; set; }
        private FileState _state;
        public FileState State
        {
            get => _state;
            set
            {
                _state = value;
                StrongReferenceMessenger.Default.Send(new FileStateChangedMessage(_state, this));
            }
        }

        private long _bytesTransmitted;
        public long BytesTransmitted
        {
            get => _bytesTransmitted;
            set
            {
                lock (_bytesTransmittedLock)
                {
                    _bytesTransmitted = value;
                    if (_bytesTransmittedNotificationTimer == null)
                    {
                        StrongReferenceMessenger.Default.Send(new BytesTransmittedChangedMessage(_bytesTransmitted, this));
                        _bytesTransmittedNotificationTimer = new Timer(SendBytesTransmittedUpdate, null, 500, Timeout.Infinite);
                    }
                }
            }
        }

        public FileDownload(Guid fileId, string name, long size)
        {
            Id = fileId;
            Name = name;
            Size = size;
            _state = FileState.Paused;
        }

        private void SendBytesTransmittedUpdate(object? state)
        {
            lock (_bytesTransmittedLock)
            {
                if (_bytesTransmitted != _bytesTransmittedLastValue)
                {
                    StrongReferenceMessenger.Default.Send(new BytesTransmittedChangedMessage(_bytesTransmitted, this));
                    _bytesTransmittedLastValue = _bytesTransmitted;
                }
                _bytesTransmittedNotificationTimer?.Change(500, Timeout.Infinite);
            }
        }
    }
}
