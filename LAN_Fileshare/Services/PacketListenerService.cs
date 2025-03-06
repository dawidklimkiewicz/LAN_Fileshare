using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public class PacketListenerService
    {
        private readonly AppStateStore _appStateStore;

        private readonly PacketService _packetService;
        private readonly TcpListener _packetListener;
        private readonly CancellationTokenSource _packetListenerCancellationToken = new();

        public PacketListenerService(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
            _packetService = new();
            _packetListener = new(IPAddress.Any, appStateStore.PacketListenerPort);
        }

        public async Task Start()
        {
            try
            {
                _packetListener.Start();

                while (!_packetListenerCancellationToken.Token.IsCancellationRequested)
                {
                    Trace.WriteLine("WAITING FOR CONNECTION");

                    using TcpClient tcpClient = await _packetListener.AcceptTcpClientAsync();
                    Trace.WriteLine("ACCEPTED CLIENT");

                    using NetworkStream networkStream = tcpClient.GetStream();
                    ReadPacket(networkStream);

                    // Signal that the stream can be closed
                    byte[] acknowledgePacket = _packetService.CreateAcknowledgePacket();
                    networkStream.Write(acknowledgePacket);
                    networkStream.Flush();
                }
            }

            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void ReadPacket(NetworkStream networkStream)
        {
            PacketType packetType = _packetService.ReadPacketType(networkStream);
            Trace.WriteLine($"Packet type: {packetType}");

            switch (packetType)
            {
                case PacketType.Ping: ProcessPingPacket(networkStream); break;
                case PacketType.HostInfo: ProcessHostInfoPacket(networkStream); break;
                case PacketType.HostInfoReply: ProcessHostInfoReplyPacket(networkStream); break;
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private void ProcessPingPacket(NetworkStream networkStream)
        {
            IPAddress remoteIp = _packetService.ReadPingPacket(networkStream);

            try
            {
                using TcpClient tcpClient = new();
                tcpClient.Connect(remoteIp, _appStateStore.PacketListenerPort);
                using NetworkStream responseStream = tcpClient.GetStream();

                byte[] hostInfoPacket = _packetService.CreateHostInfoPacket(_appStateStore.IPAddress, _appStateStore.PhysicalAddress, _appStateStore.Username);
                byte[] responseBuffer = new byte[1];

                responseStream.Write(hostInfoPacket, 0, hostInfoPacket.Length);
                responseStream.Flush();
                responseStream.ReadExactly(responseBuffer, 0, responseBuffer.Length);

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error answering to Ping received from {remoteIp} : {ex}");
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private void ProcessHostInfoPacket(NetworkStream networkStream)
        {
            var packetFields = _packetService.ReadHostInfoPacket(networkStream);
            IPAddress remoteIp = packetFields.SenderIp;
            PhysicalAddress remotePhysicalAddress = packetFields.PhysicalAddress;
            string remoteUsername = packetFields.Username;

            if (!_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
            {
                Host newHost = new(remotePhysicalAddress, remoteIp, remoteUsername);
                _appStateStore.HostStore.AddHost(newHost);
            }

            try
            {
                using TcpClient tcpClient = new();
                tcpClient.Connect(remoteIp, _appStateStore.PacketListenerPort);
                using NetworkStream responseStream = tcpClient.GetStream();

                byte[] hostInfoReplyPacket = _packetService.CreateHostInfoReplyPacket(_appStateStore.IPAddress, _appStateStore.PhysicalAddress, _appStateStore.Username);
                byte[] responseBuffer = new byte[1];

                responseStream.Write(hostInfoReplyPacket, 0, hostInfoReplyPacket.Length);
                responseStream.Flush();
                responseStream.ReadExactly(responseBuffer, 0, responseBuffer.Length);

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error answering to HostInfo received from {remoteIp} : {ex}");
            }
        }

        private void ProcessHostInfoReplyPacket(NetworkStream networkStream)
        {
            var packetFields = _packetService.ReadHostInfoPacket(networkStream);
            IPAddress remoteIp = packetFields.SenderIp;
            PhysicalAddress remotePhysicalAddress = packetFields.PhysicalAddress;
            string remoteUsername = packetFields.Username;

            if (!_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
            {
                Host newHost = new(remotePhysicalAddress, remoteIp, remoteUsername);
                _appStateStore.HostStore.AddHost(newHost);
            }
        }

        public void Stop()
        {
            _packetListenerCancellationToken.Cancel();
            _packetListener.Dispose();
        }
    }
}
