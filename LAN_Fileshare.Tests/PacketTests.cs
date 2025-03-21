using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Tests
{
    [TestFixture]
    public class PacketTests
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public async Task CreateAndReadHostInfoPacket_Match_True()
        {
            IPAddress ip = IPAddress.Parse("192.168.100.1");
            PhysicalAddress physicalAddress = new([11, 22, 33, 44, 55, 66]);
            string username = "Pc username";
            byte[] packet = PacketService.Create.HostInfo(ip, physicalAddress, username);

            await TestPacketTransmission(packet, PacketService.Read.HostInfo, receivedPacket =>
            {
                Assert.That(receivedPacket.SenderIp, Is.EqualTo(ip));
                Assert.That(receivedPacket.PhysicalAddress, Is.EqualTo(physicalAddress));
                Assert.That(receivedPacket.Username, Is.EqualTo(username));
            });
        }

        [Test]
        public async Task CreateAndReadFileInfoPacket_Match_True()
        {
            IPAddress senderIP = IPAddress.Parse("192.168.100.1");
            List<FileUpload> files = new()
            {
                new FileUpload(Guid.NewGuid(), "file1.txt", 1024),
                new FileUpload(Guid.NewGuid(), "file2.pdf", 2048)
            };

            byte[] packet = PacketService.Create.FileInformation(senderIP, files);

            await TestPacketTransmission(
                packet,
                PacketService.Read.FileInformation,
                receivedPacket =>
                {
                    Assert.That(receivedPacket.senderIP, Is.EqualTo(senderIP));
                    Assert.That(receivedPacket.files.Count, Is.EqualTo(files.Count));

                    for (int i = 0; i < files.Count; i++)
                    {
                        Assert.That(receivedPacket.files[i].Id, Is.EqualTo(files[i].Id));
                        Assert.That(receivedPacket.files[i].Name, Is.EqualTo(files[i].Name));
                        Assert.That(receivedPacket.files[i].Size, Is.EqualTo(files[i].Size));
                        Assert.That(receivedPacket.files[i].TimeCreated, Is.EqualTo(files[i].TimeCreated));
                        Assert.That(receivedPacket.files[i].BytesTransmitted, Is.EqualTo(files[i].BytesTransmitted));
                    }
                }
            );
        }

        [Test]
        public async Task CreateAndReadRemoveFilePacket_Match_True()
        {
            IPAddress senderIP = IPAddress.Parse("192.168.100.1");
            Guid fileId = Guid.NewGuid();

            byte[] packet = PacketService.Create.RemoveFile(senderIP, fileId);

            await TestPacketTransmission(packet, PacketService.Read.RemoveFile, receivedPacket =>
            {
                Assert.That(receivedPacket.senderIP, Is.EqualTo(senderIP));
                Assert.That(receivedPacket.fileId, Is.EqualTo(fileId));
            });
        }

        [Test]
        public async Task CreateAndReadFileRequestPacket_Match_True()
        {
            IPAddress senderIP = IPAddress.Parse("192.168.100.1");
            int listenerPort = 12345;
            Guid fileId = Guid.NewGuid();
            long startingByte = 23908572;

            byte[] packet = PacketService.Create.FileRequest(senderIP, listenerPort, fileId, startingByte);

            await TestPacketTransmission(packet, PacketService.Read.FileRequest, receivedPacket =>
            {
                Assert.That(receivedPacket.senderIP, Is.EqualTo(senderIP));
                Assert.That(receivedPacket.listenerPort, Is.EqualTo(listenerPort));
                Assert.That(receivedPacket.fileId, Is.EqualTo(fileId));
                Assert.That(receivedPacket.startingByte, Is.EqualTo(startingByte));
            });
        }

        [Test]
        public async Task CreateAndReadFileDataPacket_Match_True()
        {
            byte[] data = [1, 2, 1, 1, 5, 1, 3, 5, 6, 7, 1, 3, 4, 1, 3];
            long dataLength = data.Length;

            byte[] packet = PacketService.Create.FileData(data);

            await TestPacketTransmission(packet, PacketService.Read.FileData, receivedPacket =>
            {
                Assert.That(receivedPacket, Is.EqualTo(data));
            });
        }


        // Checks if sent and received packets match
        private async Task TestPacketTransmission<T>(byte[] packetToSend, Func<NetworkStream, T> packetReader, Action<T> assertPacket)
        {
            using TcpListener listener = new(IPAddress.Loopback, 53777);
            listener.Start();

            Task serverTask = Task.Run(async () =>
            {
                using TcpClient server = await listener.AcceptTcpClientAsync();
                using NetworkStream serverStream = server.GetStream();
                PacketType packetType = PacketService.Read.PacketType(serverStream);
                T receivedPacket = packetReader(serverStream);
                assertPacket(receivedPacket);

                byte[] acknowledgePacket = PacketService.Create.Acknowledge();
                await serverStream.WriteAsync(acknowledgePacket);
                await serverStream.FlushAsync();
            });

            Task clientTask = Task.Run(async () =>
            {
                using TcpClient client = new();
                await client.ConnectAsync(IPAddress.Loopback, 53777);
                using NetworkStream clientStream = client.GetStream();

                await clientStream.WriteAsync(packetToSend, 0, packetToSend.Length);
                await clientStream.FlushAsync();

                byte[] responseBuffer = new byte[1];
                clientStream.ReadExactly(responseBuffer, 0, responseBuffer.Length);
                client.Close();

            });

            await Task.WhenAll(clientTask, serverTask);
        }
    }
}
