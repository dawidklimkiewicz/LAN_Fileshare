using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class FileDialogService
    {
        private readonly Window _window;

        public FileDialogService(Window window)
        {
            _window = window;
        }

        public async Task<string[]?> OpenFileDialogAsync()
        {
            var storageProvider = _window.StorageProvider;
            if (storageProvider == null)
            {
                return null;
            }

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = true,
            });

            return files.Select(file => file.Path.LocalPath).ToArray();
        }
    }
}
