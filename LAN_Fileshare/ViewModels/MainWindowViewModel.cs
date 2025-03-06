using CommunityToolkit.Mvvm.ComponentModel;
using LAN_Fileshare.Stores;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly AppStateStore _appStateStore;
        private ViewModelBase currentViewModel;

        [ObservableProperty]
        private IPAddress _localIPAddress;

        [ObservableProperty]
        private string _localUsername;

        public HostListingViewModel HostListingViewModel { get; set; }

        public MainWindowViewModel(AppStateStore appStateStore)
        {
            currentViewModel = new();

            _appStateStore = appStateStore;
            LocalIPAddress = _appStateStore.IPAddress;
            LocalUsername = _appStateStore.Username;

            HostListingViewModel = new(appStateStore);

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        // TODO wyzerować wszystko po utracie połączenia z siecią
        private void NetworkChange_NetworkAddressChanged(object? sender, System.EventArgs e)
        {
            // Read local IP after reconnecting
            _appStateStore.InitLocalUserInfo();
            LocalIPAddress = _appStateStore.IPAddress;
        }
    }
}
