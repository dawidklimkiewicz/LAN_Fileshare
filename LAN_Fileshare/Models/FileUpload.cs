using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using System;
using System.IO;
using System.Threading;

namespace LAN_Fileshare.Models
{
    public class FileUpload : IFile
    {
        private Timer? _bytesTransmittedNotificationTimer;
        private object _bytesTransmittedLock = new();
        private long _bytesTransmittedLastValue;
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; } = "";
        public long Size { get; set; }
        public DateTime TimeCreated { get; set; }

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

        public FileUpload(string filePath)
        {
            Id = Guid.NewGuid();
            FileInfo fileInfo = new(filePath);
            Path = filePath;
            Name = fileInfo.Name;
            Size = fileInfo.Length;
            TimeCreated = DateTime.Now;
            _bytesTransmitted = 0;
            _state = FileState.Paused;
        }

        public FileUpload(Guid id, string path, DateTime timeCreated, long bytesTransmitted)
        {
            FileInfo fileInfo = new(path);
            Id = id;
            Name = fileInfo.Name;
            Path = path;
            Size = fileInfo.Length;
            TimeCreated = timeCreated;
            _bytesTransmitted = bytesTransmitted;
            _state = _bytesTransmitted == Size ? FileState.Finished : FileState.Paused;
        }

        // For testing purposes
        public FileUpload(Guid id, string name, long size)
        {
            Id = id;
            Name = name;
            Size = size;
            TimeCreated = DateTime.Now;
            BytesTransmitted = 1000;
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
