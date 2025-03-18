using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using LAN_Fileshare.ViewModels;
using System.IO;
using System.Linq;

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

        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            var viewmodel = (FileListingViewModel?)DataContext;
            string[]? filePaths = files?.Select(f => f.Path.LocalPath).Where(path => !Directory.Exists(path)).ToArray() ?? null;

            if (viewmodel != null && filePaths != null)
            {
                viewmodel.DropFilesCommand.Execute(filePaths);
            }
        }
    }
}