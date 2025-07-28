using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.Stores
{
    public class AppStateStore
    {
        private PhysicalAddress? _physicalAddress = null!;
        public PhysicalAddress? PhysicalAddress => _physicalAddress;

        private IPAddress? _ipAddress = null!;
        public IPAddress? IPAddress => _ipAddress;
        public IPAddress? IPMask = null;
        public string Username
        {
            get => SettingsStore.Username;
            set => SettingsStore.Username = value;
        }

        public int PacketListenerPort { get; set; } = 53788;
        public SettingsStore SettingsStore { get; set; }
        public HostStore HostStore { get; set; }
        public Host? SelectedHost { get; set; }

        public AppStateStore()
        {
            HostStore = new HostStore();
            SettingsStore = new SettingsStore(this);
            SettingsStore.Load();
            InitLocalUserInfo();
        }

        public void InitLocalUserInfo()
        {
            NetworkService networkService = new(this);
            var userInfo = networkService.GetLocalUserInfo();
            _ipAddress = userInfo.ipAddress;
            IPMask = userInfo.iPMask;
            _physicalAddress = userInfo.physicalAddress;

            StrongReferenceMessenger.Default.Send(new NetworkInfoUpdated(this));
        }

        public void DisposeNetworkInformation()
        {
            _ipAddress = null;
            _physicalAddress = null;
            StrongReferenceMessenger.Default.Send(new NetworkInfoUpdated(this));
        }
    }
}
