using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
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
        private TcpListener _packetListener = null!;
        CancellationTokenSource? _packetListenerCancellationTokenSource = null!;

        public PacketListenerService(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
        }

        public void Start()
        {
            if (_packetListener != null) Stop(); // prevents multiple runs

            _packetListenerCancellationTokenSource = new();
            _packetListener = new(IPAddress.Any, _appStateStore.PacketListenerPort);
            _packetListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _ = Task.Run(() => StartListener(_packetListenerCancellationTokenSource.Token));
        }

        public async Task StartListener(CancellationToken token)
        {
            try
            {
                _packetListener.Start();

                while (!token.IsCancellationRequested)
                {
                    if (_packetListener == null || _packetListener.Server?.IsBound == false)
                    {
                        Start();
                        return;
                    }

                    Trace.WriteLine("WAITING FOR CONNECTION");
                    try
                    {
                        TcpClient tcpClient = await _packetListener.AcceptTcpClientAsync(token);
                        Trace.WriteLine("ACCEPTED CLIENT");

                        _ = Task.Run(() => HandleClientAsync(tcpClient));
                    }
                    catch (OperationCanceledException)
                    {
                        Trace.WriteLine("Listener stopped.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error accepting client: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Listener error: {ex}");
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            try
            {
                using NetworkStream networkStream = tcpClient.GetStream();
                await ReadPacket(networkStream);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error handling client: {ex}");
            }
            finally
            {
                tcpClient.Close();
            }
        }

        private async Task ReadPacket(NetworkStream networkStream)
        {
            PacketType packetType = PacketService.Read.PacketType(networkStream);
            Trace.WriteLine($"Packet type: {packetType}");

            switch (packetType)
            {
                case PacketType.Ping: ProcessPingPacket(networkStream); break;
                case PacketType.HostInfo: ProcessHostInfoPacket(networkStream); break;
                case PacketType.HostInfoReply: ProcessHostInfoReplyPacket(networkStream); break;
                case PacketType.FileInformation: ProcessFileInfoPacket(networkStream); break;
                case PacketType.RemoveFile: ProcessRemoveFilePacket(networkStream); break;
                case PacketType.FileRequest: ProcessFileRequestPacket(networkStream); break;
            }

            try
            {
                // Signal that the stream can be closed
                byte[] acknowledgePacket = PacketService.Create.Acknowledge();
                await networkStream.WriteAsync(acknowledgePacket);
                await networkStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send acknowledgement packet - {ex}");
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private async void ProcessPingPacket(NetworkStream networkStream)
        {
            IPAddress remoteIp = PacketService.Read.Ping(networkStream);

            try
            {
                using TcpClient tcpClient = new();
                tcpClient.Connect(remoteIp, _appStateStore.PacketListenerPort);
                using NetworkStream responseStream = tcpClient.GetStream();

                byte[] hostInfoPacket = PacketService.Create.HostInfo(_appStateStore.IPAddress!, _appStateStore.PhysicalAddress!, _appStateStore.Username);
                byte[] responseBuffer = new byte[1];

                await responseStream.WriteAsync(hostInfoPacket, 0, hostInfoPacket.Length);
                await responseStream.FlushAsync();
                responseStream.ReadExactly(responseBuffer, 0, responseBuffer.Length);

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error answering to Ping received from {remoteIp} : {ex}");
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private async void ProcessHostInfoPacket(NetworkStream networkStream)
        {
            var packetFields = PacketService.Read.HostInfo(networkStream);
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

                byte[] hostInfoReplyPacket = PacketService.Create.HostInfoReply(_appStateStore.IPAddress!, _appStateStore.PhysicalAddress!, _appStateStore.Username);
                byte[] responseBuffer = new byte[1];

                await responseStream.WriteAsync(hostInfoReplyPacket, 0, hostInfoReplyPacket.Length);
                await responseStream.FlushAsync();
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
            var packetFields = PacketService.Read.HostInfo(networkStream);
            IPAddress senderIP = packetFields.SenderIp;
            PhysicalAddress remotePhysicalAddress = packetFields.PhysicalAddress;
            string remoteUsername = packetFields.Username;

            if (!_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
            {
                Host newHost = new(remotePhysicalAddress, senderIP, remoteUsername);
                _appStateStore.HostStore.AddHost(newHost);
            }
        }

        private void ProcessFileInfoPacket(NetworkStream networkStream)
        {
            var packetFields = PacketService.Read.FileInformation(networkStream);
            IPAddress senderIP = packetFields.senderIP;
            List<FileDownload> files = packetFields.files;

            Host? host = _appStateStore.HostStore.Get(senderIP);
            if (host != null)
            {
                host.FileDownloadList.AddRange(files);
            }
        }

        private void ProcessRemoveFilePacket(NetworkStream netowrkStream)
        {
            var packetFields = PacketService.Read.RemoveFile(netowrkStream);
            IPAddress senderIP = packetFields.senderIP;
            Guid fileId = packetFields.fileId;

            Host? host = _appStateStore.HostStore.Get(senderIP);
            if (host != null)
            {
                FileUpload? fileUpload = host.FileUploadList.Get(fileId);
                FileDownload? fileDownload = host.FileDownloadList.Get(fileId);

                if (fileUpload != null)
                {
                    host.FileUploadList.Remove(fileUpload);
                }

                if (fileDownload != null)
                {
                    host.FileDownloadList.Remove(fileDownload);
                }
            }
        }

        private void ProcessFileRequestPacket(NetworkStream networkStream)
        {
            var packetFields = PacketService.Read.FileRequest(networkStream);
            IPAddress senderIP = packetFields.senderIP;
            Guid fileId = packetFields.fileId;
            int listenerPort = packetFields.listenerPort;
            long startingByte = packetFields.startingByte;

            Host? host = _appStateStore.HostStore.Get(senderIP);
            FileUpload? file = host?.FileUploadList.Get(fileId);

            if (host != null && file != null)
            {
                FileDataSender sender = new(host, file, listenerPort, startingByte);
                Task senderTask = sender.StartSending();
                _appStateStore.ActiveFileTransfers.Add(senderTask);
            }
        }

        public void Stop()
        {
            if (_packetListenerCancellationTokenSource != null && !_packetListenerCancellationTokenSource.IsCancellationRequested)
            {
                _packetListenerCancellationTokenSource.Cancel();
                _packetListenerCancellationTokenSource = null;
            }

            if (_packetListener != null)
            {
                _packetListener.Stop();
                _packetListener = null!;
            }
        }
    }
}
