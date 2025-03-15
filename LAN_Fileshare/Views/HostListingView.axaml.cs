using Avalonia.Controls;
using LAN_Fileshare.Services;

namespace LAN_Fileshare.Views;

public partial class HostListingView : UserControl
{
    public HostListingView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            DataContext = new ViewModels.DesignTime.MainViewModel(new FileDialogService(new Avalonia.Controls.Window()));
        }
    }
}