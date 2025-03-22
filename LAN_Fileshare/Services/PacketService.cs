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
            /// <para>Each file = (16B) Id | (8B) Size | (4B) Name length | (X Bytes) Name | (64 B) Time created | (8 B) Bytes transmitted</para> 
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
                    fields.Add(BitConverter.GetBytes(file.TimeCreated.ToBinary()));
                    fields.Add(BitConverter.GetBytes(file.BytesTransmitted));
                }

                return SerializePacket(PacketType.FileInformation, fields);
            }

            /// <summary>
            /// <para>(1B) Type | (4B) IPAddress | (4B) File Count | (X Bytes) Files</para>
            /// <para>Each file = (16B) Id | (8B) Size | (4B) Name length | (X Bytes) Name | (64 B) Time created | (8 B) Bytes transmitted</para>
            /// </summary>
            public static byte[] InitialFileInformation(IPAddress senderIP, List<FileUpload> files)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), BitConverter.GetBytes(files.Count)];

                foreach (FileUpload file in files)
                {
                    fields.Add(file.Id.ToByteArray());
                    fields.Add(BitConverter.GetBytes(file.Size));
                    fields.Add(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(file.Name)));
                    fields.Add(Encoding.UTF8.GetBytes(file.Name));
                    fields.Add(BitConverter.GetBytes(file.TimeCreated.ToBinary()));
                    fields.Add(BitConverter.GetBytes(file.BytesTransmitted));
                }

                return SerializePacket(PacketType.InitialFileInformation, fields);
            }

            /// <summary>
            /// <para>(1B) Type | (4B) IPAddress | (4B) File Count | (X Bytes) Files</para>
            /// <para>Each file = (16B) Id | (8B) Size | (4B) Name length | (X Bytes) Name | (64 B) Time created | (8 B) Bytes transmitted</para>
            /// </summary>
            public static byte[] InitialFileInformationReply(IPAddress senderIP, List<FileUpload> files)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), BitConverter.GetBytes(files.Count)];

                foreach (FileUpload file in files)
                {
                    fields.Add(file.Id.ToByteArray());
                    fields.Add(BitConverter.GetBytes(file.Size));
                    fields.Add(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(file.Name)));
                    fields.Add(Encoding.UTF8.GetBytes(file.Name));
                    fields.Add(BitConverter.GetBytes(file.TimeCreated.ToBinary()));
                    fields.Add(BitConverter.GetBytes(file.BytesTransmitted));
                }

                return SerializePacket(PacketType.InitialFileInformationReply, fields);
            }

            /// <summary>
            /// (1B) Type | (4B) IPAddress | (16B) File id  
            /// </summary>
            public static byte[] RemoveFile(IPAddress senderIP, Guid fileId)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), fileId.ToByteArray()];
                return SerializePacket(PacketType.RemoveFile, fields);
            }

            /// <summary>
            /// (1B) Type | (4B) IPAddress | (4B) Listener port | (16B) File id  | (8B) Starting byte 
            /// </summary>
            public static byte[] FileRequest(IPAddress senderIP, int listenerPort, Guid fileId, long startingByte)
            {
                List<byte[]> fields = [senderIP.GetAddressBytes(), BitConverter.GetBytes(listenerPort), fileId.ToByteArray(), BitConverter.GetBytes(startingByte)];
                return SerializePacket(PacketType.FileRequest, fields);
            }

            /// <summary>
            /// (1B) Type | (4B) Data length | (X B) File data 
            /// </summary>
            public static byte[] FileData(byte[] data)
            {
                List<byte[]> fields = [BitConverter.GetBytes(data.Length), data];
                return SerializePacket(PacketType.FileData, fields);
            }

            /// <summary>
            /// (1B) Type | (8B) Last received byte 
            /// </summary>
            public static byte[] FileDataAck(long lastReceivedByte)
            {
                List<byte[]> fields = [BitConverter.GetBytes(lastReceivedByte)];
                return SerializePacket(PacketType.FileDataAck, fields);
            }

            /// <summary>
            /// (1B) Type | (8B) Last received byte 
            /// </summary>
            public static byte[] StopFileTransmission(long lastReceivedByte)
            {
                List<byte[]> fields = [BitConverter.GetBytes(lastReceivedByte)];
                return SerializePacket(PacketType.StopFileTransmission, fields);
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

            // TODO extract reading method, similar to creating
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
                    byte[] timeCreatedBuffer = new byte[8];
                    byte[] bytesTransmittedBuffer = new byte[8];

                    networkStream.ReadExactly(fileIdBuffer);
                    networkStream.ReadExactly(fileSizeBuffer);
                    networkStream.ReadExactly(fileNameSizeBuffer);

                    Guid id = new Guid(fileIdBuffer);
                    long size = BitConverter.ToInt64(fileSizeBuffer);
                    int fileNameSize = BitConverter.ToInt32(fileNameSizeBuffer);
                    byte[] fileNameBuffer = new byte[fileNameSize];

                    networkStream.ReadExactly(fileNameBuffer);
                    string fileName = Encoding.UTF8.GetString(fileNameBuffer);

                    networkStream.ReadExactly(timeCreatedBuffer);
                    DateTime timeCreated = DateTime.FromBinary(BitConverter.ToInt64(timeCreatedBuffer));

                    networkStream.ReadExactly(bytesTransmittedBuffer);
                    long bytesTransmitted = BitConverter.ToInt64(bytesTransmittedBuffer);

                    receivedFiles.Add(new FileDownload(id, fileName, size, timeCreated, bytesTransmitted));
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

            public static (IPAddress senderIP, int listenerPort, Guid fileId, long startingByte) FileRequest(NetworkStream networkStream)
            {
                byte[] senderIpBuffer = new byte[4];
                byte[] listenerPortBuffer = new byte[4];
                byte[] fileIdBuffer = new byte[16];
                byte[] startingByteBuffer = new byte[8];

                networkStream.ReadExactly(senderIpBuffer, 0, senderIpBuffer.Length);
                networkStream.ReadExactly(listenerPortBuffer, 0, listenerPortBuffer.Length);
                networkStream.ReadExactly(fileIdBuffer, 0, fileIdBuffer.Length);
                networkStream.ReadExactly(startingByteBuffer, 0, startingByteBuffer.Length);

                return (new IPAddress(senderIpBuffer), BitConverter.ToInt32(listenerPortBuffer), new Guid(fileIdBuffer), BitConverter.ToInt64(startingByteBuffer));
            }

            public static byte[] FileData(NetworkStream networkStream)
            {
                byte[] dataLengthBuffer = new byte[4];
                networkStream.ReadExactly(dataLengthBuffer, 0, dataLengthBuffer.Length);
                long dataLength = BitConverter.ToInt32(dataLengthBuffer);

                byte[] dataBuffer = new byte[dataLength];
                networkStream.ReadExactly(dataBuffer, 0, dataBuffer.Length);

                return dataBuffer;
            }

            public static long FileDataAck(NetworkStream networkStream)
            {
                byte[] lastReceivedByteBuffer = new byte[8];
                networkStream.ReadExactly(lastReceivedByteBuffer, 0, lastReceivedByteBuffer.Length);
                return BitConverter.ToInt64(lastReceivedByteBuffer);
            }

            public static long StopFileTransmission(NetworkStream networkStream)
            {
                byte[] lastReceivedByteBuffer = new byte[8];
                networkStream.ReadExactly(lastReceivedByteBuffer, 0, lastReceivedByteBuffer.Length);
                return BitConverter.ToInt64(lastReceivedByteBuffer);
            }
        }
    }
}
