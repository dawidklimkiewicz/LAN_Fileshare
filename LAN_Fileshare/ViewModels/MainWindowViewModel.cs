using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.EntityFramework;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using System;
using System.Net;

namespace LAN_Fileshare.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IRecipient<NetworkInfoUpdated>, IDisposable
    {
        private readonly AppStateStore _appStateStore;

        [ObservableProperty]
        private IPAddress? _localIPAddress;

        [ObservableProperty]
        private string _localUsername;

        public HostListingViewModel HostListingViewModel { get; set; }
        public FileListingViewModel FileListingViewModel { get; set; }

        public MainWindowViewModel(AppStateStore appStateStore, FileDialogService fileDialogService, MainDbContextFactory mainDbContextFactory)
        {
            _appStateStore = appStateStore;
            LocalIPAddress = _appStateStore.IPAddress;
            LocalUsername = _appStateStore.Username;

            HostListingViewModel = new(appStateStore);
            FileListingViewModel = new(appStateStore, fileDialogService, mainDbContextFactory);

            StrongReferenceMessenger.Default.Register<NetworkInfoUpdated>(this);
        }

        public void Receive(NetworkInfoUpdated message)
        {
            LocalIPAddress = message.Value.IPAddress;
        }

        public void Dispose()
        {
            StrongReferenceMessenger.Default.Unregister<NetworkInfoUpdated>(this);
        }
    }
}
