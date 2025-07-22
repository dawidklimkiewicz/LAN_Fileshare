using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public class HostConnection : IDisposable
    {
        public TcpClient Client { get; set; }
        public bool IsConnected;
        private NetworkStream _networkStream;
        private TimeSpan _keepAliveInterval = TimeSpan.FromSeconds(3);
        private TimeSpan _keepAliveTimeout = TimeSpan.FromSeconds(2);
        private int _timeouts = 0;
        private CancellationTokenSource _cts = new();
        public event Action? Disconnected;

        public HostConnection(TcpClient client)
        {
            Client = client;
            _networkStream = Client.GetStream();
            _ = Task.Run(() => StartKeepAlive(_cts.Token));
        }

        private async Task StartKeepAlive(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] packet = PacketService.Create.KeepAlive();
                    byte[] responseBuffer = new byte[1];

                    await _networkStream.WriteAsync(packet, token);
                    await _networkStream.FlushAsync(token);

                    var readTask = _networkStream.ReadExactlyAsync(responseBuffer, token).AsTask();
                    var timeoutTask = Task.Delay(_keepAliveTimeout, token);

                    var completedTask = await Task.WhenAny(readTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        IsConnected = false;
                        _timeouts++;

                        if (_timeouts >= 3)
                        {
                            throw new TimeoutException("Keepalive packet timed out");
                        }
                    }
                    else
                    {
                        PacketType packetType = (PacketType)responseBuffer[0];
                        if (packetType != PacketType.Acknowledge)
                        {
                            throw new Exception($"Unexpected response to keepalive: {packetType}");
                        }
                        else if (!IsConnected)
                        {
                            IsConnected = true;
                            _timeouts = 0;
                        }
                    }

                    await Task.Delay(_keepAliveInterval, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Trace.WriteLine($"Keepalive error: {ex}");
                Disconnect();
            }
        }

        public void Disconnect()
        {
            _cts.Cancel();
            Disconnected?.Invoke();
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
            Client.Close();
            _cts?.Dispose();
        }
    }
}
