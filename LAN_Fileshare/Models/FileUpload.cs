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
        public DateTime? TimeFinished { get; set; }
        public FileState FileState { get; set; } = FileState.Paused;
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
            FileInfo fileInfo = new FileInfo(filePath);
            Path = filePath;
            Name = fileInfo.Name;
            Size = fileInfo.Length;
            TimeCreated = DateTime.Now;
            _bytesTransmitted = 0;
        }

        public FileUpload(Guid id, string name, long size)
        {
            Id = id;
            Name = name;
            Size = size;
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
