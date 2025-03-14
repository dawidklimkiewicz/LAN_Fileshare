using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class HostCheck
    {
        private readonly AppStateStore _appStateStore;
        private CancellationTokenSource? _monitorHostsCancellationTokenSource = null!;

        public HostCheck(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
        }

        public void Start()
        {
            if (_monitorHostsCancellationTokenSource != null && !_monitorHostsCancellationTokenSource.IsCancellationRequested) return;

            _monitorHostsCancellationTokenSource = new();
            _ = Task.Run(() => MonitorHosts(_monitorHostsCancellationTokenSource.Token));
        }

        public void Stop()
        {
            _monitorHostsCancellationTokenSource?.Cancel();
            _monitorHostsCancellationTokenSource?.Dispose();
            _monitorHostsCancellationTokenSource = null;
        }

        public async Task MonitorHosts(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    List<Task> keepAliveTasks = new();

                    foreach (Host host in _appStateStore.HostStore.GetHostList())
                    {
                        if (token.IsCancellationRequested) return;
                        keepAliveTasks.Add(SendKeepAlive(host, token));
                    }

                    await Task.WhenAll(keepAliveTasks);
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Trace.WriteLine($"HostCheck error: {ex}");
            }
        }

        private async Task SendKeepAlive(Host host, CancellationToken token)
        {
            try
            {
                using TcpClient tcpClient = new();
                var connectTask = tcpClient.ConnectAsync(host.IPAddress, _appStateStore.PacketListenerPort);

                if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(4), token)) != connectTask)
                    throw new TimeoutException();

                tcpClient.Close();
            }
            catch
            {
                _appStateStore.HostStore.RemoveHost(host);
            }
        }
    }
}
