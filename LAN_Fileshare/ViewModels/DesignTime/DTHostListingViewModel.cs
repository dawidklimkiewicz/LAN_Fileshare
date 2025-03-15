using LAN_Fileshare.Models;
using LAN_Fileshare.Stores;
using System.Net;
using System.Net.NetworkInformation;

namespace LAN_Fileshare.ViewModels.DesignTime
{
    public partial class DTHostListingViewModel : HostListingViewModel
    {
        public DTHostListingViewModel() : base(new AppStateStore())
        {
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "longusernamelongusernamelongusername")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
            _hostListingItemViewModels.Add(new HostListingItemViewModel(new Host(new PhysicalAddress([10, 10, 10, 10, 10, 10]), new IPAddress([192, 168, 100, 101]), "Test")));
        }
    }
}
