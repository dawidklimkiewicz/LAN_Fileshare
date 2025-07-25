using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class NetworkService
    {
        private CancellationTokenSource? _pingPeriodicallyCancellationTokenSource = null!;
        private readonly AppStateStore _appStateStore;

        public NetworkService(AppStateStore appStateStore)
        {
            _appStateStore = appStateStore;
        }

        public void GetLocalUserInfo(ref PhysicalAddress? physicalAddress, ref IPAddress? ipAddress, ref IPAddress? iPMask, ref string username)
        {
            username = Environment.UserName;

            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                if (NetworkInterface.GetAllNetworkInterfaces().Length == 0)
                {
                    throw new InvalidOperationException("[GET NETWORK INTERFACE]: No network interfaces were found");
                }

                NetworkInterface? networkInterface = GetNetworkInterface();

                if (networkInterface == null)
                {
                    throw new InvalidOperationException("[GET NETWORK INTERFACE]: No network interfaces were found");
                }

                physicalAddress = networkInterface.GetPhysicalAddress();
                ipAddress = networkInterface.GetIPProperties().UnicastAddresses
                    .Where(nic => nic.Address.AddressFamily == AddressFamily.InterNetwork).First().Address;
                iPMask = networkInterface.GetIPProperties().UnicastAddresses
                    .Where(nic => nic.Address.AddressFamily == AddressFamily.InterNetwork).First().IPv4Mask;
            }

            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex.Message);
                ipAddress = null;
                physicalAddress = null;
            }
        }

        public NetworkInterface? GetNetworkInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(x => x?.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && x?.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                        && x?.OperationalStatus == OperationalStatus.Up
                        && !x.Name.Contains("vEthernet")
                        && !x.Description.Contains("VirtualBox")
                        && x.Speed > 0
                        && !x.IsReceiveOnly, null);
        }

        /// <summary>
        /// Discovers new hosts in the local network by broadcasting Ping
        /// </summary>
        public async Task PingNetwork()
        {
            if (_appStateStore.IPAddress == null || _appStateStore.IPMask == null)
            {
                throw new InvalidOperationException("[PING NETWORK]: No valid network interface");
            }

            uint networkAddressBinary = IpAddressToBinary(GetNetworkAddress(_appStateStore.IPAddress, _appStateStore.IPMask));
            uint broadcastAddressBinary = IpAddressToBinary(GetBroadcastAddress(_appStateStore.IPAddress, _appStateStore.IPMask));

            List<Task> pingTasks = new();

            List<uint> existingHostsIpList = _appStateStore.HostStore.GetHostList().Select(host => IpAddressToBinary(host.IPAddress)).ToList();
            for (uint address = networkAddressBinary + 1; address < broadcastAddressBinary; address++)
            {
                if (existingHostsIpList.Contains(address))
                {
                    continue;
                }

                string ipAddressString = $"{(address >> 24) & 255}.{(address >> 16) & 255}.{(address >> 8) & 255}.{address & 255}";
                IPAddress pingAddress = IPAddress.Parse(ipAddressString);

                // if (pingAddress.Equals(_appStateStore.IPAddress)) continue;

                pingTasks.Add(TryConnection(pingAddress, _appStateStore.PacketListenerPort));
            }
            await Task.WhenAll(pingTasks);
        }

        public void StartPingingPeriodically()
        {
            if (_pingPeriodicallyCancellationTokenSource != null && !_pingPeriodicallyCancellationTokenSource.IsCancellationRequested) return; // prevents multiple runs

            Trace.WriteLine($"STARTED PINGING");
            _pingPeriodicallyCancellationTokenSource = new();
            _ = Task.Run(() => PingPeriodically(_pingPeriodicallyCancellationTokenSource.Token));
        }

        public void StopPingingPeriodically()
        {
            if (_pingPeriodicallyCancellationTokenSource == null) return;

            _pingPeriodicallyCancellationTokenSource.Cancel();
            _pingPeriodicallyCancellationTokenSource.Dispose();
            _pingPeriodicallyCancellationTokenSource = null;

            Trace.WriteLine($"STOPPED PINGING");
        }

        private async Task PingPeriodically(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await PingNetwork().ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
            }
            catch { }
        }

        private async Task TryConnection(IPAddress ip, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(ip, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] pingPacket = PacketService.Create.Ping(_appStateStore.IPAddress!);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(pingPacket, 0, pingPacket.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (SocketException) { }
            catch (Exception) { }
        }

        public async Task SendInitialFileInformation(List<FileUpload> files, IPAddress receiverIP, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(receiverIP, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] packet = PacketService.Create.InitialFileInformation(_appStateStore.IPAddress!, files);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(packet, 0, packet.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send file information - {ex}");
            }
        }

        public async Task SendInitialFileInformationReply(List<FileUpload> files, IPAddress receiverIP, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(receiverIP, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] packet = PacketService.Create.InitialFileInformationReply(_appStateStore.IPAddress!, files);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(packet, 0, packet.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send file information - {ex}");
            }
        }

        public async Task SendFileInformation(List<FileUpload> files, IPAddress receiverIP, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(receiverIP, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] packet = PacketService.Create.FileInformation(_appStateStore.IPAddress!, files);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(packet, 0, packet.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send file information - {ex}");
            }
        }
        public async Task SendRemoveFile(Guid fileId, IPAddress receiverIP, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(receiverIP, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] packet = PacketService.Create.RemoveFile(_appStateStore.IPAddress!, fileId);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(packet, 0, packet.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send file information - {ex}");
            }
        }

        private IPAddress GetNetworkAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();
            byte[] networkAddressBytes = new byte[ipAddressBytes.Length];

            for (int i = 0; i < ipAddressBytes.Length; i++)
            {
                networkAddressBytes[i] = (byte)(ipAddressBytes[i] & subnetMaskBytes[i]);
            }

            return new IPAddress(networkAddressBytes);
        }

        private IPAddress GetBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();
            byte[] broadcastAddressBytes = new byte[ipAddressBytes.Length];

            for (int i = 0; i < ipAddressBytes.Length; i++)
            {
                broadcastAddressBytes[i] = (byte)(ipAddressBytes[i] | ~subnetMaskBytes[i]);
            }

            return new IPAddress(broadcastAddressBytes);
        }

        private uint IpAddressToBinary(IPAddress ip)
        {
            byte[] addressBytes = ip.GetAddressBytes();
            string ipBinaryString = string.Join("", addressBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
            return Convert.ToUInt32(ipBinaryString, 2);
        }
    }
}
