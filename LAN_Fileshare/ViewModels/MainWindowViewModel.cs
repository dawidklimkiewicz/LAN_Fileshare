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

        public MainWindowViewModel(AppStateStore appStateStore)
        {
            currentViewModel = new();

            _appStateStore = appStateStore;
            LocalIPAddress = _appStateStore.IPAddress;
            LocalUsername = _appStateStore.Username;

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private void NetworkChange_NetworkAddressChanged(object? sender, System.EventArgs e)
        {
            // Pobierz nowy adres
            _appStateStore.InitLocalUserInfo();
            LocalIPAddress = _appStateStore.IPAddress;
        }
    }
}
