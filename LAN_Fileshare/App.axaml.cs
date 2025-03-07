using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using LAN_Fileshare.ViewModels;
using LAN_Fileshare.Views;
using System.Linq;
using System.Net.NetworkInformation;

namespace LAN_Fileshare
{
    public partial class App : Application
    {
        private AppStateStore _appStateStore = new();
        private PacketListenerService _packetListenerService = null!;
        private NetworkService _networkService = null!;
        private HostCheck _hostCheckService = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            _packetListenerService = new(_appStateStore);
            _networkService = new(_appStateStore);
            _hostCheckService = new(_appStateStore);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            _packetListenerService.Start();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_appStateStore),
                };
            }

            _networkService.StartPingingPeriodically();
            _hostCheckService.Start();

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            base.OnFrameworkInitializationCompleted();
        }

        private void NetworkChange_NetworkAddressChanged(object? sender, System.EventArgs e)
        {
            NetworkInterface? networkInterface = _networkService.GetNetworkInterface();

            if (networkInterface == null)
            {
                _packetListenerService.Stop();
                _networkService.StopPingingPeriodically();
                _hostCheckService.Stop();

                _appStateStore.DisposeNetworkInformation();
                _appStateStore.HostStore.RemoveAllHosts();
            }

            else
            {
                // Read network information after reconnecting
                _appStateStore.InitLocalUserInfo();

                _packetListenerService.Start();
                _networkService.StartPingingPeriodically();
                _hostCheckService.Start();
            }
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}