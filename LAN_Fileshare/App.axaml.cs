using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LAN_Fileshare.EntityFramework;
using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using LAN_Fileshare.ViewModels;
using LAN_Fileshare.Views;
using Microsoft.EntityFrameworkCore;
using System;
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
        private MainWindow _mainWindow = null!;
        private MainDbContextFactory _mainDbContextFactory = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            string connectionString = "Data Source=database.db";
            _mainDbContextFactory = new(new DbContextOptionsBuilder().UseSqlite(connectionString).Options);

            using (MainDbContext context = _mainDbContextFactory.Create())
            {
                context.Database.Migrate();
            }

            _packetListenerService = new(_appStateStore, _mainDbContextFactory);
            _networkService = new(_appStateStore);
            _hostCheckService = new(_appStateStore);

        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                _mainWindow = new();
                _mainWindow.DataContext = new MainWindowViewModel(_appStateStore, new FileDialogService(_mainWindow), _mainDbContextFactory);
                _mainWindow.Closing += _mainWindow_Closing;
                desktop.MainWindow = _mainWindow;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }

            _packetListenerService.Start();
            //_networkService.StartPingingPeriodically();
            await _networkService.PingNetwork();
            _hostCheckService.Start();

            NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            base.OnFrameworkInitializationCompleted();
        }

        private void _mainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true;
            _mainWindow.Hide();
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

        private void NativeMenuItem_Open(object? sender, System.EventArgs e)
        {
            _mainWindow.Show();
        }

        private void NativeMenuItem_Close(object? sender, System.EventArgs e)
        {
            // TODO signal shutdown to other hosts
            Environment.Exit(0);
        }

        private void TrayIcon_Clicked(object? sender, System.EventArgs e)
        {
            _mainWindow.Show();
        }
    }
}