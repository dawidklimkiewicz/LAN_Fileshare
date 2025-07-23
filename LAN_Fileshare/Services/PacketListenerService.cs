using LAN_Fileshare.EntityFramework;
using LAN_Fileshare.EntityFramework.Queries.FileUpload;
using LAN_Fileshare.EntityFramework.Queries.Host;
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
        private readonly MainDbContextFactory _mainDbContextFactory;
        private TcpListener _packetListener = null!;
        CancellationTokenSource? _packetListenerCancellationTokenSource = null!;

        public PacketListenerService(AppStateStore appStateStore, MainDbContextFactory mainDbContextFactory)
        {
            _appStateStore = appStateStore;
            _mainDbContextFactory = mainDbContextFactory;
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
                        Trace.WriteLine($"ACCEPTED CLIENT {tcpClient.Client.RemoteEndPoint}");

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
                await ReadPacket(tcpClient);
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

        private async Task ReadPacket(TcpClient tcpClient)
        {
            using NetworkStream networkStream = tcpClient.GetStream();
            PacketType packetType = PacketService.Read.PacketType(networkStream);
            Trace.WriteLine($"Packet type: {packetType}");

            switch (packetType)
            {
                case PacketType.Ping: await ProcessPingPacket(tcpClient); break;
                case PacketType.FileInformation: ProcessFileInfoPacket(networkStream); break;
                case PacketType.RemoveFile: ProcessRemoveFilePacket(networkStream); break;
                case PacketType.FileRequest: ProcessFileRequestPacket(networkStream); break;
                case PacketType.InitialFileInformation: ProcessInitialFileInformationPacket(networkStream); break;
                case PacketType.InitialFileInformationReply: ProcessFileInfoPacket(networkStream); break;
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private async Task ProcessPingPacket(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            IPAddress remoteIp = PacketService.Read.Ping(networkStream);
            try
            {
                PacketType packetType;

                // Send HostInfo
                byte[] hostInfoPacket = PacketService.Create.HostInfo(_appStateStore.IPAddress!, _appStateStore.PhysicalAddress!, _appStateStore.Username);
                Trace.WriteLine("HS - Sending HostInfo");
                await networkStream.WriteAsync(hostInfoPacket);
                await networkStream.FlushAsync();

                // Read HostInfoReply and create Host object
                packetType = PacketService.Read.PacketType(networkStream);
                Trace.WriteLine($"HS - Received {packetType}");
                if (packetType != PacketType.HostInfoReply)
                {
                    throw new Exception($"Expected HostInfoReply packet, got {packetType} instead");
                }

                var hostInfoReplyFields = PacketService.Read.HostInfo(networkStream);
                IPAddress remoteIP = hostInfoReplyFields.SenderIp;
                PhysicalAddress remotePhysicalAddress = hostInfoReplyFields.PhysicalAddress;
                string remoteUsername = hostInfoReplyFields.Username;

                if (_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
                {
                    Trace.WriteLine($"Tried to add a host that already exists: {remoteIP}");
                    tcpClient.Dispose();
                    return;
                }

                GetOrCreateHost getOrCreateHost = new(_mainDbContextFactory);
                Host? newHost = await getOrCreateHost.Execute(remotePhysicalAddress, remoteIP, remoteUsername, tcpClient);
                Trace.WriteLine($"HS - Created new host: {newHost.IPAddress}");

                if (newHost == null)
                {
                    Trace.WriteLine($"Error creating host {remoteIP}");
                    tcpClient.Dispose();
                    return;
                }
                _appStateStore.HostStore.AddHost(newHost);

                // Send InitialFileInformation
                byte[] initialFileInformationPacket = PacketService.Create.InitialFileInformation(_appStateStore.IPAddress!, newHost.FileUploadList.ToList());
                Trace.Write("HS - Sendnig fileinfo");
                await networkStream.WriteAsync(initialFileInformationPacket);
                await networkStream.FlushAsync();

                // Read InitialFileInformationReply
                packetType = PacketService.Read.PacketType(networkStream);
                Trace.WriteLine($"HS - received {packetType}");
                if (packetType != PacketType.InitialFileInformationReply)
                {
                    Trace.WriteLine($"Expected InitialFileInformationReply packet, got {packetType} instead");
                    tcpClient.Dispose();
                    return;
                }

                var fileInfoReplyFields = PacketService.Read.FileInformation(networkStream);
                List<FileDownload> files = fileInfoReplyFields.files;

                newHost.FileDownloadList.AddRange(files);
                Trace.WriteLine("HS - Files added, handshake finished");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error answering to Ping received from {remoteIp} : {ex}");
            }
        }

        // TODO: dodać timeout albo nawet powtórzenie wysłania
        private async void ProcessHostInfoPacket(TcpClient tcpClient)
        {
            using NetworkStream networkStream = tcpClient.GetStream();
            var packetFields = PacketService.Read.HostInfo(networkStream);
            IPAddress remoteIP = packetFields.SenderIp;
            PhysicalAddress remotePhysicalAddress = packetFields.PhysicalAddress;
            string remoteUsername = packetFields.Username;

            if (!_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
            {
                try
                {
                    GetOrCreateHost getOrCreateHost = new(_mainDbContextFactory);
                    Host newHost = await getOrCreateHost.Execute(remotePhysicalAddress, remoteIP, remoteUsername, tcpClient);
                    _appStateStore.HostStore.AddHost(newHost);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error adding host to store: {ex}");
                }
            }

            try
            {
                byte[] hostInfoReplyPacket = PacketService.Create.HostInfoReply(_appStateStore.IPAddress!, _appStateStore.PhysicalAddress!, _appStateStore.Username);
                byte[] responseBuffer = new byte[1];

                await networkStream.WriteAsync(hostInfoReplyPacket);
                await networkStream.FlushAsync();
                networkStream.ReadExactly(responseBuffer, 0, responseBuffer.Length);

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error answering to HostInfo received from {remoteIP} : {ex}");
            }
        }

        //private async void ProcessHostInfoReplyPacket(NetworkStream networkStream)
        //{
        //    var packetFields = PacketService.Read.HostInfo(networkStream);
        //    IPAddress remoteIP = packetFields.SenderIp;
        //    PhysicalAddress remotePhysicalAddress = packetFields.PhysicalAddress;
        //    string remoteUsername = packetFields.Username;

        //    if (!_appStateStore.HostStore.ContainsHost(remotePhysicalAddress))
        //    {
        //        try
        //        {
        //            GetOrCreateHost getOrCreateHost = new(_mainDbContextFactory);
        //            Host newHost = await getOrCreateHost.Execute(remotePhysicalAddress, remoteIP, remoteUsername);
        //            _appStateStore.HostStore.AddHost(newHost);

        //            NetworkService networkService = new(_appStateStore);
        //            await networkService.SendInitialFileInformation(newHost.FileUploadList.ToList(), remoteIP, _appStateStore.PacketListenerPort);
        //        }
        //        catch (Exception ex)
        //        {
        //            Trace.WriteLine($"Error receiving HostInfoReply packet: {ex}");
        //        }
        //    }
        //}

        private async void ProcessInitialFileInformationPacket(NetworkStream netowrkStream)
        {
            var packetFields = PacketService.Read.FileInformation(netowrkStream);
            IPAddress senderIP = packetFields.senderIP;
            List<FileDownload> files = packetFields.files;

            Host? host = _appStateStore.HostStore.Get(senderIP);
            if (host != null)
            {
                host.FileDownloadList.AddRange(files);

                NetworkService networkService = new(_appStateStore, _mainDbContextFactory);
                await networkService.SendInitialFileInformationReply(host.FileUploadList.ToList(), senderIP, _appStateStore.PacketListenerPort);
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

        private async void ProcessRemoveFilePacket(NetworkStream netowrkStream)
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
                    try
                    {
                        host.FileUploadList.Remove(fileUpload);
                        DeleteFileUpload deleteFileUpload = new(_mainDbContextFactory);
                        await deleteFileUpload.Execute(fileUpload.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error removing FileUpload from DB: {ex}");
                    }
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
                FileDataSender sender = new(host, file, listenerPort, startingByte, _mainDbContextFactory);
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
