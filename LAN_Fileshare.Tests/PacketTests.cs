using LAN_Fileshare.Services;
using System.Diagnostics;
using System.Net;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Tests
{
    [TestFixture]
    public class PacketTests
    {
        private PacketService _packetService;
        private IPAddress _ipAddress;

        [SetUp]
        public void Setup()
        {
            _packetService = new();
            _ipAddress = new([192, 168, 1, 1]);
        }

        [Test]
        public void CreatePingPacket_ExecutionTimeOK_True()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using MemoryStream memoryStream = new();
            using BinaryWriter packetData = new(memoryStream);

            packetData.Write((byte)PacketType.Ping);
            packetData.Write(_ipAddress.GetAddressBytes());

            //byte[] buffer = new byte[5];
            //Span<byte> span = buffer;

            //span[0] = (byte)PacketType.Ping;
            //_ipAddress.GetAddressBytes().CopyTo(span.Slice(1, 4));

            long time = stopwatch.ElapsedTicks;
            TestContext.Out.WriteLine(time);

            Assert.That(time, Is.LessThan(1000));
        }
    }
}
