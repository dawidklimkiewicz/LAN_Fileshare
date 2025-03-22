using Avalonia.Controls;

namespace LAN_Fileshare.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            // Clears search box focus when clicking anywhere else
            FocusManager?.ClearFocus();
        }
    }
}