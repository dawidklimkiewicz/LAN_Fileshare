using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace LAN_Fileshare.Views;

public partial class FileListingView : UserControl
{
    IBrush? dragOverBrush;
    public FileListingView()
    {
        InitializeComponent();
        dragOverBrush = (IBrush?)Application.Current?.FindResource("DragHoverBackgroundColor");
    }

    private void FileUploadsList_DragOver(object sender, DragEventArgs e)
    {
        if (dragOverBrush != null)
        {
            FileUploadsListBox.Background = dragOverBrush;
        }
    }

    private void FileUploadsList_DragLeave(object sender, DragEventArgs e)
    {
        FileUploadsListBox.Background = Brushes.Transparent;
    }

    private void FileUploadsList_Drop(object sender, DragEventArgs e)
    {
        FileUploadsListBox.Background = Brushes.Transparent;
    }
}