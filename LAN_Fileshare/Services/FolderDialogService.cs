using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.Services
{
    public class FolderDialogService
    {
        private readonly Window _window;
        public FolderDialogService(Window window)
        {
            _window = window;
        }

        public async Task<string?> OpenFolderDialog()
        {
            try
            {
                IStorageProvider storageProvider = _window.StorageProvider;
                if (storageProvider == null)
                {
                    return null;
                }

                var dir = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false });
                return dir.Select(path => path.Path.LocalPath).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error ocurred while opening folder dialog - {ex}");
                return null;
            }
        }
    }
}
