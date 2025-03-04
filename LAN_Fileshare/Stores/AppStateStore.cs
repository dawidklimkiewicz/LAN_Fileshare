using LAN_Fileshare.Models;
using LAN_Fileshare.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.Stores
{
    public class AppStateStore
    {
        private PhysicalAddress _physicalAddress = null!;
        private IPAddress _ipAddress = null!;
        private string _username = null!;

        public PhysicalAddress PhysicalAddress => _physicalAddress;
        public IPAddress IPAddress => _ipAddress;
        public string Username => _username;

        public List<Host> AvailableHosts;

        public AppStateStore()
        {
            AvailableHosts = new();

            InitLocalUserInfo();
        }

        public void InitLocalUserInfo()
        {
            NetworkService networkService = new();
            networkService.GetLocalUserInfo(ref _physicalAddress, ref _ipAddress, ref _username);
        }

    }
}
