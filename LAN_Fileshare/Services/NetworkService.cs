using LAN_Fileshare.Stores;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class NetworkService
    {
        AppStateStore _appStateStore;
        private List<uint> _existingHostsIpList => _appStateStore.HostStore.GetHostList().Select(host => IpAddressToBinary(host.IPAddress)).ToList();

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

                if (networkInterfaces.Length == 0)
                {
                    throw new InvalidOperationException("[GET NETWORK INTERFACE]: No network interfaces were found");
                }

                NetworkInterface? networkInterface = networkInterfaces.FirstOrDefault(x => x?.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && x?.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                        && x?.OperationalStatus == OperationalStatus.Up
                        && !x.Name.Contains("vEthernet")
                        && !x.Description.Contains("VirtualBox"), null);

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

        public async Task PingNetwork()
        {
            if (_appStateStore.IPAddress == null || _appStateStore.IPMask == null)
            {
                throw new InvalidOperationException("[PING NETWORK]: No valid network interface");
            }

            uint networkAddressBinary = IpAddressToBinary(GetNetworkAddress(_appStateStore.IPAddress, _appStateStore.IPMask));
            uint broadcastAddressBinary = IpAddressToBinary(GetBroadcastAddress(_appStateStore.IPAddress, _appStateStore.IPMask));

            List<Task> pingTasks = new List<Task>();

            for (uint address = networkAddressBinary + 1; address < broadcastAddressBinary; address++)
            {
                if (_existingHostsIpList.Contains(address))
                {
                    continue;
                }

                string ipAddressString = $"{(address >> 24) & 255}.{(address >> 16) & 255}.{(address >> 8) & 255}.{address & 255}";
                IPAddress pingAddress = IPAddress.Parse(ipAddressString);

                if (pingAddress.Equals(_appStateStore.IPAddress)) continue;

                pingTasks.Add(TryConnection(pingAddress, _appStateStore.PacketListenerPort));
            }
            await Task.WhenAll(pingTasks);
        }

        private async Task TryConnection(IPAddress ip, int port)
        {
            try
            {
                using TcpClient tcpClient = new();
                await tcpClient.ConnectAsync(ip, port);
                using NetworkStream networkStream = tcpClient.GetStream();

                byte[] pingPacket = PacketService.CreatePingPacket(_appStateStore.IPAddress!);
                byte[] ackBuffer = new byte[1];

                await networkStream.WriteAsync(pingPacket, 0, pingPacket.Length);
                await networkStream.FlushAsync();

                networkStream.ReadExactly(ackBuffer, 0, ackBuffer.Length);
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to send ping packet  - {ex}");
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
