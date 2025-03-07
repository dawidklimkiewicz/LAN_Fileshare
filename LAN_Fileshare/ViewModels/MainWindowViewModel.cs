using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Stores;
using System.Net;

namespace LAN_Fileshare.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NetworkInfoUpdated>
    {
        private readonly AppStateStore _appStateStore;
        private ViewModelBase currentViewModel;

        [ObservableProperty]
        private IPAddress? _localIPAddress;

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
        }

        public void Receive(NetworkInfoUpdated message)
        {
            LocalIPAddress = message.Value.IPAddress;
        }
    }
}
