using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Services;
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
        public FileListingViewModel FileListingViewModel { get; set; }

        public MainWindowViewModel(AppStateStore appStateStore, FileDialogService fileDialogService)
        {
            currentViewModel = new();

            _appStateStore = appStateStore;
            LocalIPAddress = _appStateStore.IPAddress;
            LocalUsername = _appStateStore.Username;

            HostListingViewModel = new(appStateStore);
            FileListingViewModel = new(appStateStore, fileDialogService);
        }

        public void Receive(NetworkInfoUpdated message)
        {
            LocalIPAddress = message.Value.IPAddress;
        }
    }
}
