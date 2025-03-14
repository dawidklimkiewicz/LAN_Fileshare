using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public Host? Get(IPAddress ipAddress)
        {
            lock (HostListLock)
            {
                return _hosts.FirstOrDefault(host => host!.IPAddress.Equals(ipAddress), null);
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
            List<Host> removedHosts = new();
            lock (HostListLock)
            {
                removedHosts = _hosts.ToList();
                _hosts.Clear();
            }

            foreach (Host host in removedHosts)
            {
                OnHostRemoved(host);
            }
        }

        public IReadOnlyList<Host> GetHostList()
        {
            lock (HostListLock)
            {
                return _hosts.ToList();
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
