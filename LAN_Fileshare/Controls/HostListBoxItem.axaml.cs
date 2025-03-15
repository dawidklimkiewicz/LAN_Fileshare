using Avalonia;
using Avalonia.Controls;

namespace LAN_Fileshare.Controls;

public class HostListBoxItem : ListBoxItem
{
    public static readonly StyledProperty<string> HostnameProperty =
        AvaloniaProperty.Register<HostListBoxItem, string>(nameof(Hostname));

    public string Hostname
    {
        get => this.GetValue(HostnameProperty);
        set => SetValue(HostnameProperty, value);
    }

    public static readonly StyledProperty<string> HostIpProperty =
        AvaloniaProperty.Register<HostListBoxItem, string>(nameof(HostIp));

    public string HostIp
    {
        get => this.GetValue(HostIpProperty);
        set => SetValue(HostIpProperty, value);
    }


}