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
                    }
                }
            );
        }

        [Test]
        public async Task RemoveFile_CorrectlyReadsPacket()
        {
            IPAddress expectedSenderIp = IPAddress.Parse("192.168.1.10");
            Guid expectedFileId = Guid.NewGuid();

            byte[] senderIpBytes = expectedSenderIp.GetAddressBytes();
            byte[] fileIdBytes = expectedFileId.ToByteArray();
            byte[] packet = senderIpBytes.Concat(fileIdBytes).ToArray();

            using TcpListener listener = new(IPAddress.Loopback, 53778);
            listener.Start();

            Task serverTask = Task.Run(async () =>
            {
                using TcpClient server = await listener.AcceptTcpClientAsync();
                using NetworkStream serverStream = server.GetStream();

                var (senderIp, fileId) = PacketService.Read.RemoveFile(serverStream);

                Assert.That(senderIp, Is.EqualTo(expectedSenderIp));
                Assert.That(fileId, Is.EqualTo(expectedFileId));
            });

            Task clientTask = Task.Run(async () =>
            {
                using TcpClient client = new();
                await client.ConnectAsync(IPAddress.Loopback, 53778);
                using NetworkStream clientStream = client.GetStream();

                await clientStream.WriteAsync(packet, 0, packet.Length);
                await clientStream.FlushAsync();
            });

            await Task.WhenAll(clientTask, serverTask);
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
