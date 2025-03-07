﻿using CommunityToolkit.Mvvm.Messaging;
using LAN_Fileshare.Messages;
using LAN_Fileshare.Services;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace LAN_Fileshare.Stores
{
    public class AppStateStore
    {
        private PhysicalAddress? _physicalAddress = null!;
        private IPAddress? _ipAddress = null!;

        public PhysicalAddress? PhysicalAddress => _physicalAddress;
        public IPAddress? IPAddress => _ipAddress;
        public IPAddress? IPMask = null!;
        public string Username = "";

        public int PacketListenerPort { get; set; } = 53788;
        public HostStore HostStore { get; set; }

        public CancellationTokenSource PingPeriodicallyCancellationToken = new();
        public CancellationTokenSource MonitorHostsCancellationToken = new();
        public CancellationTokenSource PacketListenerCancellationToken = new();

        public AppStateStore()
        {
            InitLocalUserInfo();
            HostStore = new HostStore();
        }

        public void InitLocalUserInfo()
        {
            NetworkService networkService = new(this);
            networkService.GetLocalUserInfo(ref _physicalAddress, ref _ipAddress, ref IPMask, ref Username);
            StrongReferenceMessenger.Default.Send(new NetworkInfoUpdated(this));
        }

    }
}
