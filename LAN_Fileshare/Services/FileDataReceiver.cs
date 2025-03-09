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
    public class FileDataReceiver : IDisposable
    {
        private readonly string _temporaryDownloadPath;
        private readonly BinaryWriter _fileWriter;
        private readonly AppStateStore _appStateStore;
        private FileDownload _fileToDownload;
        private Host _fileOwnerHost;
        private TcpClient _fileOwnerTcpClient = null!;
        private NetworkStream _fileOwnerNetworkStream = null!;
        private CancellationToken _ct;
        private TcpListener _fileDataListener = null!;
        private int _listenerPort;
        private object _writerLock = new();


        public FileDataReceiver(AppStateStore appStateStore, Host fileOwnerHost, FileDownload fileToDownload)
        {
            _fileOwnerHost = fileOwnerHost;
            _fileToDownload = fileToDownload;
            _appStateStore = appStateStore;

            _ct = new();
            _temporaryDownloadPath = Path.Combine(appStateStore.TemporaryDownloadDirectory, fileToDownload.Id.ToString());
            CreateDownloadFolder();
            _fileWriter = new BinaryWriter(new FileStream(_temporaryDownloadPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            _fileWriter.BaseStream.Seek(_fileToDownload.BytesTransmitted, SeekOrigin.Begin);
        }

        public async Task SendFileRequest()
        {
            _fileToDownload.State = FileState.Transmitting;
            try
            {
                _listenerPort = FindAvailablePort();

                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(_fileOwnerHost.IPAddress, _appStateStore.PacketListenerPort);
                using NetworkStream networkStream = tcpClient.GetStream();


                byte[] packet = PacketService.Create.FileRequest(_appStateStore.IPAddress!, _listenerPort, _fileToDownload.Id, _fileToDownload.BytesTransmitted);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(packet, _ct);
                await networkStream.FlushAsync(_ct);

                await networkStream.ReadExactlyAsync(ackBuffer, _ct);
                tcpClient.Close();

                _fileDataListener = new(IPAddress.Any, _listenerPort);

                _ = Task.Run(() => StartListener(), _ct);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending file request: {ex}");
            }
        }

        private async Task StartListener()
        {
            try
            {
                _fileDataListener.Start();
                _fileOwnerTcpClient = await _fileDataListener.AcceptTcpClientAsync(_ct);
                _fileOwnerNetworkStream = _fileOwnerTcpClient.GetStream();

                byte[] packetTypeBuffer = new byte[1];

                while (!_ct.IsCancellationRequested)
                {
                    await _fileOwnerNetworkStream.ReadExactlyAsync(packetTypeBuffer, _ct);

                    if ((PacketType)packetTypeBuffer[0] != PacketType.FileData) break;

                    byte[] fileData = PacketService.Read.FileData(_fileOwnerNetworkStream);

                    long lastReceivedByte = _fileToDownload.BytesTransmitted + fileData.Length;
                    byte[] responsePacket = PacketService.Create.FileDataAck(lastReceivedByte);

                    WriteFileData(fileData);
                    if (lastReceivedByte >= _fileToDownload.Size) break;
                    await _fileOwnerNetworkStream.WriteAsync(responsePacket, _ct);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error receiving file data: {ex}");
            }
            finally
            {
                if (_fileOwnerTcpClient != null)
                {
                    Trace.WriteLine($"Sending StopTransmission packet for {_fileToDownload.Name}");
                    byte[] packet = PacketService.Create.StopFileTransmission(_fileToDownload.BytesTransmitted);
                    await _fileOwnerNetworkStream.WriteAsync(packet, _ct);

                    byte[] ackBuffer = new byte[1];
                    await _fileOwnerNetworkStream.ReadExactlyAsync(ackBuffer);
                }

                Dispose();
            }
        }

        private void WriteFileData(byte[] data)
        {
            lock (_writerLock)
            {
                _fileWriter.Write(data);
                _fileToDownload.BytesTransmitted += data.LongLength;
            }
        }

        private int FindAvailablePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void CreateDownloadFolder()
        {
            string path = _appStateStore.TemporaryDownloadDirectory;
            if (!Directory.Exists(path))
            {
                DirectoryInfo dir = Directory.CreateDirectory(path);
                dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
        }

        public void Dispose()
        {
            _fileDataListener.Stop();
            _fileDataListener?.Dispose();
            _fileOwnerNetworkStream?.Dispose();
            _fileOwnerTcpClient?.Dispose();
            _fileWriter.Dispose();

            Trace.WriteLine($"Finished downloading {_fileToDownload.Name}");

            if (_fileToDownload.Size == _fileToDownload.BytesTransmitted)
            {
                _fileToDownload.TimeFinished = DateTime.Now;
                _fileToDownload.State = FileState.Finished;

                // TODO check if file already exists and if it does then add an index

                try
                {
                    File.Move(Path.Combine(_temporaryDownloadPath), Path.Combine(_appStateStore.DownloadDirectory, _fileToDownload.Name));
                    Trace.WriteLine($"Moved {_fileToDownload.Name} to {Path.Combine(_appStateStore.DownloadDirectory, _fileToDownload.Name)}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Error moving file: {ex.Message}");
                }
            }
            else
            {
                _fileToDownload.State = FileState.Paused;
            }
        }
    }
}
