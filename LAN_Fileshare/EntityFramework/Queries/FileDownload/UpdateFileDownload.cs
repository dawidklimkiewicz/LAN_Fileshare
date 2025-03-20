using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileDownload
{
    public class UpdateFileDownload
    {
        private readonly MainDbContextFactory _contextFactory;

        public UpdateFileDownload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.FileDownload file)
        {
            using MainDbContext context = _contextFactory.Create();
            FileDownloadDto? dto = await context.FileDownloads.FindAsync(file.Id);

            if (dto is null)
            {
                throw new InvalidOperationException("File download not found.");
            }

            dto.BytesTransmitted = file.BytesTransmitted;
            dto.TimeFinished = file.TimeFinished;

            context.FileDownloads.Update(dto);
            await context.SaveChangesAsync();
        }
    }
}
