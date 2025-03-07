using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using static LAN_Fileshare.Models.PacketTypes;

namespace LAN_Fileshare.Services
{
    public static class PacketService
    {
        private static byte[] SerializePacket(PacketType packetType, List<byte[]> fields)
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

        /// <summary>
        /// Reads packet's first byte
        /// </summary>
        public static PacketType ReadPacketType(NetworkStream packetData)
        {
            byte[] packetTypeBuffer = new byte[1];
            int bytesRead = packetData.Read(packetTypeBuffer, 0, 1);
            return (PacketType)packetTypeBuffer[0];
        }

        public static byte[] CreateAcknowledgePacket()
        {
            return SerializePacket(PacketType.Acknowledge, []);
        }

        public static byte[] CreateKeepAlivePacket()
        {
            return SerializePacket(PacketType.KeepAlive, []);
        }

        /// <summary>
        /// (1B) Type | (4B) IPAddress
        /// </summary>
        public static byte[] CreatePingPacket(IPAddress senderIP)
        {
            List<byte[]> fields = [senderIP.GetAddressBytes()];
            return SerializePacket(PacketType.Ping, fields);
        }

        public static IPAddress ReadPingPacket(NetworkStream packetData)
        {
            byte[] senderAddressBuffer = new byte[4];
            packetData.ReadExactly(senderAddressBuffer, 0, 4);
            return new IPAddress(senderAddressBuffer);
        }

        /// <summary>
        /// (1B) Type | (4B) IPAddress | (6B) MAC address | (4B) Username length | (X Bytes) Username
        /// </summary>
        public static byte[] CreateHostInfoPacket(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
        {
            List<byte[]> fields = [
                senderIP.GetAddressBytes(),
                physicalAddress.GetAddressBytes(),
                BitConverter.GetBytes(username.Length),
                Encoding.UTF8.GetBytes(username)];

            return SerializePacket(PacketType.HostInfo, fields);
        }

        public static (IPAddress SenderIp, PhysicalAddress PhysicalAddress, string Username) ReadHostInfoPacket(NetworkStream packetData)
        {
            byte[] ipBuffer = new byte[4];
            byte[] physicalAddressBuffer = new byte[6];
            byte[] usernameLengthBuffer = new byte[4];

            packetData.ReadExactly(ipBuffer);
            IPAddress ip = new(ipBuffer);

            packetData.ReadExactly(physicalAddressBuffer);
            PhysicalAddress physicalAddress = new(physicalAddressBuffer);

            packetData.ReadExactly(usernameLengthBuffer);
            int usernameLength = BitConverter.ToInt32(usernameLengthBuffer, 0);
            byte[] usernameBuffer = new byte[usernameLength];
            packetData.ReadExactly(usernameBuffer);
            string username = Encoding.UTF8.GetString(usernameBuffer, 0, usernameBuffer.Length);

            return (ip, physicalAddress, username);
        }


        /// <summary>
        /// (1B) Type | (4B) IPAddress | (6B) MAC address | (4B) Username length | (X Bytes) Username
        /// </summary>
        public static byte[] CreateHostInfoReplyPacket(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
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
