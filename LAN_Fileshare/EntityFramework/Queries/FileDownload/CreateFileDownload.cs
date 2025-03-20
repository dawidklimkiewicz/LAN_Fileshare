using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileDownload
{
    public class CreateFileDownload
    {
        private readonly MainDbContextFactory _contextFactory;

        public CreateFileDownload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.Host host, Models.FileDownload fileDownload)
        {
            MainDbContext context = _contextFactory.Create();

            HostDto? hostDto = await context.Hosts.FindAsync(host.PhysicalAddress);
            if (hostDto == null)
            {
                throw new Exception("Host not found");
            }

            FileDownloadDto fileDownloadDto = new()
            {
                Id = fileDownload.Id,
                Host = hostDto,
                Name = fileDownload.Name,
                Size = fileDownload.Size,
                BytesTransmitted = fileDownload.BytesTransmitted,
                TimeCreated = fileDownload.TimeCreated,
                TimeFinished = fileDownload.TimeFinished
            };

            context.FileDownloads.Add(fileDownloadDto);
            await context.SaveChangesAsync();
        }
    }
}
