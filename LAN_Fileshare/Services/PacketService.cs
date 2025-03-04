using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public class PacketService
    {
        private byte[] SerializePacket(PacketType packetType, List<byte[]> fields)
        {
            using MemoryStream memoryStream = new();
            using BinaryWriter packetData = new(memoryStream);

            packetData.Write((byte)packetType);

            foreach (byte[] field in fields)
            {
                packetData.Write(field);
            }

            return memoryStream.ToArray();
        }

        public byte[] CreatePingPacket(IPAddress senderIP)
        {
            List<byte[]> fields = [senderIP.GetAddressBytes()];
            return SerializePacket(PacketType.Ping, fields);
        }

        public byte[] CreateHostInfoPacket(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
        {
            List<byte[]> fields = [
                senderIP.GetAddressBytes(),
                physicalAddress.GetAddressBytes(),
                BitConverter.GetBytes(username.Length),
                Encoding.UTF8.GetBytes(username)];

            return SerializePacket(PacketType.HostInfo, fields);
        }

        public byte[] CreateHostInfoReplyPacket(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
        {
            List<byte[]> fields = [
                senderIP.GetAddressBytes(),
                physicalAddress.GetAddressBytes(),
                BitConverter.GetBytes(username.Length),
                Encoding.UTF8.GetBytes(username)];

            return SerializePacket(PacketType.HostInfoReply, fields);
        }

    }
}
