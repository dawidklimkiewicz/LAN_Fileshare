using LAN_Fileshare.Services;
using LAN_Fileshare.Stores;
using Microsoft.EntityFrameworkCore;

namespace LAN_Fileshare.ViewModels.DesignTime
{
    public partial class MainViewModel : MainWindowViewModel
    {
        public MainViewModel(FileDialogService fileDialogService) : base(new AppStateStore(), fileDialogService,
            new EntityFramework.MainDbContextFactory(new DbContextOptionsBuilder().UseSqlite("Data Source=designtime.db").Options))
        {
            LocalIPAddress = new([192, 168, 100, 100]);
            LocalUsername = "Username";
            HostListingViewModel = new DTHostListingViewModel();
        }
    }
}
