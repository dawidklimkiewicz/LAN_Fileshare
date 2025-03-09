using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LAN_Fileshare.Models
{
    public class FileList<T> where T : IFile
    {
        private readonly object _lock = new();
        private List<T> _items = new();

        public IReadOnlyList<T> GetAll()
        {
            lock (_lock)
            {
                return _items.AsReadOnly();
            }
        }

        public T? Get(Guid id)
        {
            lock (_lock)
            {
                return _items.FirstOrDefault(file => file.Id.Equals(id));
            }
        }

        public void Add(T file)
        {
            lock (_lock)
            {
                _items.Add(file);
            }
            OnFileAdded(file);
        }

        public void AddRange(IEnumerable<T> files)
        {
            foreach (T file in files)
            {
                Add(file);
            }
        }

        public void Remove(T file)
        {
            lock (_lock)
            {
                _items.Remove(file);
            }
            OnFileRemoved(file);
        }

        private void OnFileAdded(T file)
        {
            StrongReferenceMessenger.Default.Send(new FileAddedMessage(file));
        }

        private void OnFileRemoved(T file)
        {
            StrongReferenceMessenger.Default.Send(new FileRemovedMessage(file));
        }
    }
}
