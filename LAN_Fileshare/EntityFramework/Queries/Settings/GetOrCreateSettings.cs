using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.Settings
{
    public class GetOrCreateSettings
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetOrCreateSettings(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Models.Settings> Execute(string username, string downloadPath)
        {
            using MainDbContext context = _contextFactory.Create();
            SettingsDto? settingsDto = await context.Settings.FirstOrDefaultAsync();
            Models.Settings newSettings;

            if (settingsDto == null)
            {
                newSettings = new(username, downloadPath);

                SettingsDto newSettingsDto = new()
                {
                    Username = newSettings.Username,
                    DefaultDownloadPath = newSettings.DefaultDownloadPath,
                    AutoDownloadDefault = newSettings.AutoDownloadDefault,
                    CopyUploadedFileToWorkspace = newSettings.CopyUploadedFileToWorkspace,
                    CopyUploadedFileToWorkspaceMaxSize = newSettings.CopyUploadedFileToWorkspaceMaxSize,
                };

                context.Settings.Add(newSettingsDto);
                await context.SaveChangesAsync();

                return newSettings;
            }
            else
            {
                return new Models.Settings(settingsDto.Username, settingsDto.DefaultDownloadPath, settingsDto.AutoDownloadDefault, settingsDto.CopyUploadedFileToWorkspace, settingsDto.CopyUploadedFileToWorkspaceMaxSize);
            }
        }
    }
}
