using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class HostCheck
    {
        private readonly AppStateStore _appStateStore;
        private CancellationTokenSource _monitorHostsCancellationToken = null!;

        public HostCheck(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
        }

        public void Start()
        {
            _monitorHostsCancellationToken = new();
            _ = Task.Run(() => MonitorHosts());
        }

        public void Stop()
        {
            _monitorHostsCancellationToken.Cancel();
        }

        public async Task MonitorHosts()
        {
            while (!_monitorHostsCancellationToken.Token.IsCancellationRequested)
            {
                List<Task> keepAliveTasks = new();

                foreach (Host host in _appStateStore.HostStore.GetHostList())
                {
                    keepAliveTasks.Add(SendKeepAlive(host));
                }

                await Task.WhenAll(keepAliveTasks);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        private async Task SendKeepAlive(Host host)
        {
            try
            {
                using TcpClient tcpClient = new();
                var connectTask = tcpClient.ConnectAsync(host.IPAddress, _appStateStore.PacketListenerPort);

                if (await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(2))) != connectTask)
                    throw new TimeoutException();

                using NetworkStream networkStream = tcpClient.GetStream();
                byte[] keepAlivePacket = PacketService.CreateKeepAlivePacket();
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(keepAlivePacket, 0, keepAlivePacket.Length);
                await networkStream.FlushAsync();

                var readTask = networkStream.ReadAsync(ackBuffer, 0, ackBuffer.Length);
                if (await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromSeconds(2))) != readTask)
                    throw new TimeoutException();

                int bytesRead = await readTask;
                tcpClient.Close();
            }
            catch
            {
                _appStateStore.HostStore.RemoveHost(host);
            }
        }
    }
}
