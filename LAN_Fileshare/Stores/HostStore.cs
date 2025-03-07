using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.Stores
{
    public class HostStore
    {
        public readonly object HostListLock = new object();

        private List<Host> _hosts;

        public HostStore()
        {
            _hosts = new();
        }

        public bool ContainsHost(PhysicalAddress physicalAddress)
        {
            lock (HostListLock)
            {
                return _hosts.Any(host => host.PhysicalAddress.Equals(physicalAddress));
            }
        }

        public void AddHost(Host newHost)
        {
            lock (HostListLock)
            {
                _hosts.Add(newHost);
            }

            OnHostAdded(newHost);
        }

        public void RemoveHost(Host hostToRemove)
        {
            lock (HostListLock)
            {
                _hosts.Remove(hostToRemove);
            }
            OnHostRemoved(hostToRemove);
        }

        public void RemoveAllHosts()
        {
            lock (HostListLock)
            {
                _hosts.Clear();
            }
        }

        public IReadOnlyList<Host> GetHostList()
        {
            lock (HostListLock)
            {
                return _hosts.AsReadOnly();
            }
        }

        private void OnHostAdded(Host newHost)
        {
            StrongReferenceMessenger.Default.Send(new HostAddedMessage(newHost));
        }

        private void OnHostRemoved(Host hostToRemove)
        {
            StrongReferenceMessenger.Default.Send(new HostRemovedMessage(hostToRemove));
        }
    }
}
