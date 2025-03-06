using CommunityToolkit.Mvvm.ComponentModel;
using LAN_Fileshare.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.ViewModels
{
    public partial class HostListingItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private bool isBlocked;

        public Host Host { get; }
        public IPAddress IPAddress => Host.IPAddress;
        public PhysicalAddress? PhysicalAddress => Host.PhysicalAddress;

        public HostListingItemViewModel(Host host)
        {
            Host = host;
            Username = host.Username;
            IsBlocked = host.IsBlocked;
        }
    }
}
