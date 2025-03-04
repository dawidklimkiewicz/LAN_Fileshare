using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LAN_Fileshare.Services
{
    public class NetworkService
    {
        public void GetLocalUserInfo(ref PhysicalAddress physicalAddress, ref IPAddress ipAddress, ref string username)
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
            }

            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
