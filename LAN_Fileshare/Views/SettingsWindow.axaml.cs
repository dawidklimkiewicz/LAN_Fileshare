using Avalonia.Controls;

namespace LAN_Fileshare.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void ScrollViewer_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        FocusManager?.ClearFocus();
    }
}