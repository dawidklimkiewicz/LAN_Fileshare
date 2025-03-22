using LAN_Fileshare.EntityFramework;
using LAN_Fileshare.EntityFramework.Queries.FileUpload;
using LAN_Fileshare.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public class FileDataSender : IDisposable
    {

        // TODO test larger buffer sizes
        private const int BUFFER_SIZE = 1024 * 1024;

        private readonly Host _receiverHost;
        private readonly FileUpload _fileToSend;
        private readonly int _remotePort;
        private readonly MainDbContextFactory _mainDbContextFactory;
        private CancellationTokenSource _cts = new();
        private BinaryReader _fileReader;

        public FileDataSender(Host receiverHost, FileUpload fileToSend, int remotePort, long startingByte, MainDbContextFactory mainDbContextFactory)
        {
            _receiverHost = receiverHost;
            _fileToSend = fileToSend;
            _remotePort = remotePort;
            _mainDbContextFactory = mainDbContextFactory;
            _fileReader = new BinaryReader(new FileStream(_fileToSend.Path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public async Task StartSending()
        {
            _fileToSend.State = FileState.Transmitting;

            try
            {
                byte[] dataBuffer1 = new byte[BUFFER_SIZE];
                byte[] dataBuffer2 = new byte[BUFFER_SIZE];

                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(_receiverHost.IPAddress, _remotePort);
                using NetworkStream networkStream = tcpClient.GetStream();

                _fileReader.BaseStream.Seek(_fileToSend.BytesTransmitted, SeekOrigin.Begin);
                int bytesRead = await _fileReader.BaseStream.ReadAsync(dataBuffer1, 0, dataBuffer1.Length, _cts.Token);
                if (bytesRead == 0) return;

                byte[] currentBuffer = dataBuffer1;
                byte[] nextBuffer = dataBuffer2;

                while (bytesRead > 0 && !_cts.IsCancellationRequested)
                {

                    Task<int> readTask = _fileReader.BaseStream.ReadAsync(nextBuffer, 0, nextBuffer.Length, _cts.Token);

                    byte[] packet = PacketService.Create.FileData(currentBuffer[..bytesRead]);
                    await networkStream.WriteAsync(packet, _cts.Token);
                    await networkStream.FlushAsync(_cts.Token);

                    byte[] responseBuffer = new byte[1];
                    await networkStream.ReadExactlyAsync(responseBuffer, _cts.Token);
                    PacketType packetType = (PacketType)responseBuffer[0];

                    if (packetType == PacketType.FileDataAck)
                    {
                        _fileToSend.BytesTransmitted = PacketService.Read.FileDataAck(networkStream);
                    }
                    else if (packetType == PacketType.StopFileTransmission)
                    {
                        _fileToSend.BytesTransmitted = PacketService.Read.StopFileTransmission(networkStream);
                        await networkStream.WriteAsync(PacketService.Create.Acknowledge());
                        break;
                    }

                    bytesRead = await readTask;
                    (currentBuffer, nextBuffer) = (nextBuffer, currentBuffer);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error sending file data: {ex}");
            }
            finally
            {
                Dispose();
            }
        }
        public async void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _fileReader.Dispose();

            try
            {
                UpdateFileUpload updateFileUpload = new(_mainDbContextFactory);
                await updateFileUpload.Execute(_fileToSend);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error updating file upload in database: {ex}");
            }


            if (_fileToSend.BytesTransmitted == _fileToSend.Size)
            {
                _fileToSend.State = FileState.Finished;
            }
            else
            {
                _fileToSend.State = FileState.Paused;
            }
        }
    }
}
