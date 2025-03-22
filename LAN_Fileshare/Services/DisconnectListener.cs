using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public class DisconnectListener
    {
        private readonly AppStateStore _appStateStore;
        private UdpClient _disconnectListener = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;

        public DisconnectListener(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
        }

        public void Start()
        {
            if (_disconnectListener != null) Stop();

            _cancellationTokenSource = new();
            _disconnectListener = new UdpClient(_appStateStore.DisconnectListenerPort, AddressFamily.InterNetwork);

            _ = Task.Run(() => StartDisconnectListener(_cancellationTokenSource.Token));
        }

        public async Task StartDisconnectListener(CancellationToken token)
        {
            try
            {
                using UdpClient disconnectListener = new(_appStateStore.DisconnectListenerPort, AddressFamily.InterNetwork);
                while (!token.IsCancellationRequested)
                {
                    if (_disconnectListener == null)
                    {
                        Start();
                        return;
                    }

                    UdpReceiveResult result = await _disconnectListener.ReceiveAsync();
                    _ = Task.Run(() => ReadPacket(result));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in DisconnectListener: {ex.Message}");
            }
        }

        private void ReadPacket(UdpReceiveResult data)
        {
            using MemoryStream dataStream = new(data.Buffer);
            PacketType packetType = (PacketType)dataStream.ReadByte();

            if (packetType == PacketType.Disconnect)
            {
                IPAddress remoteIP = PacketService.Read.Disconnect(dataStream);
                Host? host = _appStateStore.HostStore.Get(remoteIP);

                if (host != null)
                {
                    _appStateStore.HostStore.RemoveHost(host);
                }
                else return;
            }
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null!;
            }
            if (_disconnectListener != null)
            {
                _disconnectListener?.Close();
                _disconnectListener = null!;
            }
        }
    }
}
