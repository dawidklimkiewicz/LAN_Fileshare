using LAN_Fileshare.Models;
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
        public static class Create
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

            public static byte[] Acknowledge()
            {
                return SerializePacket(PacketType.Acknowledge, []);
            }

            /// <summary>
            /// (1B) Type | (4B) IPAddress
            /// </summary>
            public static byte[] Ping(IPAddress senderIP)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes()];
                return SerializePacket(PacketType.Ping, fields);
            }

            /// <summary>
            /// (1B) Type | (4B) IPAddress | (6B) MAC address | (4B) Username length | (X Bytes) Username
            /// </summary>
            public static byte[] HostInfo(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
            {
                List<byte[]> fields = [
                    senderIP.GetAddressBytes(),
                physicalAddress.GetAddressBytes(),
                BitConverter.GetBytes(username.Length),
                Encoding.UTF8.GetBytes(username)];

                return SerializePacket(PacketType.HostInfo, fields);
            }

            /// <summary>
            /// (1B) Type | (4B) IPAddress | (6B) MAC address | (4B) Username length | (X Bytes) Username
            /// </summary>
            public static byte[] HostInfoReply(IPAddress senderIP, PhysicalAddress physicalAddress, string username)
            {
                List<byte[]> fields = [
                    senderIP.GetAddressBytes(),
                physicalAddress.GetAddressBytes(),
                BitConverter.GetBytes(username.Length),
                Encoding.UTF8.GetBytes(username)];

                return SerializePacket(PacketType.HostInfoReply, fields);
            }

            /// <summary>
            /// <para>(1B) Type | (4B) IPAddress | (4B) File Count | (X Bytes) Files</para>
            /// <para>Each file = (16B) Id | (8B) Size | (4B) Name length | (X Bytes) Name</para>
            /// </summary>
            public static byte[] FileInformation(IPAddress senderIP, List<FileUpload> files)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), BitConverter.GetBytes(files.Count)];

                foreach (FileUpload file in files)
                {
                    fields.Add(file.Id.ToByteArray());
                    fields.Add(BitConverter.GetBytes(file.Size));
                    fields.Add(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(file.Name)));
                    fields.Add(Encoding.UTF8.GetBytes(file.Name));
                }

                return SerializePacket(PacketType.FileInformation, fields);
            }

            /// <summary>
            /// <para>(1B) Type | (4B) IPAddress | (16B) File id  
            /// </summary>
            public static byte[] RemoveFile(IPAddress senderIP, Guid fileId)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), fileId.ToByteArray()];
                return SerializePacket(PacketType.RemoveFile, fields);
            }
        }




        public static class Read
        {
            /// <summary>
            /// Reads packet's first byte
            /// </summary>
            public static PacketType PacketType(NetworkStream packetData)
            {
                byte[] packetTypeBuffer = new byte[1];
                int bytesRead = packetData.Read(packetTypeBuffer, 0, 1);
                return (PacketType)packetTypeBuffer[0];
            }

            public static IPAddress Ping(NetworkStream packetData)
            {
                byte[] senderAddressBuffer = new byte[4];
                packetData.ReadExactly(senderAddressBuffer, 0, 4);
                return new IPAddress(senderAddressBuffer);
            }

            public static (IPAddress SenderIp, PhysicalAddress PhysicalAddress, string Username) HostInfo(NetworkStream packetData)
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

            public static (IPAddress senderIP, List<FileDownload> files) FileInformation(NetworkStream networkStream)
            {
                byte[] fileCountBuffer = new byte[4];
                byte[] senderAddressBuffer = new byte[4];

                networkStream.ReadExactly(senderAddressBuffer, 0, 4);
                networkStream.ReadExactly(fileCountBuffer);
                int fileCount = BitConverter.ToInt32(fileCountBuffer);

                List<FileDownload> receivedFiles = new();

                for (int i = 0; i < fileCount; i++)
                {
                    byte[] fileIdBuffer = new byte[16];
                    byte[] fileSizeBuffer = new byte[8];
                    byte[] fileNameSizeBuffer = new byte[4];

                    networkStream.ReadExactly(fileIdBuffer);
                    networkStream.ReadExactly(fileSizeBuffer);
                    networkStream.ReadExactly(fileNameSizeBuffer);

                    Guid id = new Guid(fileIdBuffer);
                    long size = BitConverter.ToInt64(fileSizeBuffer);
                    int fileNameSize = BitConverter.ToInt32(fileNameSizeBuffer);
                    byte[] fileNameBuffer = new byte[fileNameSize];

                    networkStream.ReadExactly(fileNameBuffer);
                    string fileName = Encoding.UTF8.GetString(fileNameBuffer);

                    receivedFiles.Add(new FileDownload(id, fileName, size));
                }
                return (new IPAddress(senderAddressBuffer), receivedFiles);
            }

            public static (IPAddress senderIP, Guid fileId) RemoveFile(NetworkStream networkStream)
            {
                byte[] fileIdBuffer = new byte[16];
                byte[] senderAddressBuffer = new byte[4];

                networkStream.ReadExactly(senderAddressBuffer, 0, 4);
                networkStream.ReadExactly(fileIdBuffer);
                return (new IPAddress(senderAddressBuffer), new Guid(fileIdBuffer));

            }
        }
    }
}
