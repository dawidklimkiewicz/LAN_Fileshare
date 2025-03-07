using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using LAN_Fileshare.ViewModels;
using LAN_Fileshare.Views;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LAN_Fileshare
{
    public partial class App : Application
    {
        private AppStateStore _appStateStore = null!;
        private PacketListenerService _packetListenerService = null!;
        private Timer _pingTimer = null!;
        private NetworkService _networkService = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            _appStateStore = new();
            _packetListenerService = new(_appStateStore);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            _ = Task.Run(() => _packetListenerService.Start());

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

            _networkService = new(_appStateStore);
            _pingTimer = new Timer(async _ =>
            {
                await _networkService.PingNetwork();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            base.OnFrameworkInitializationCompleted();
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