using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.DownloadTransmissionStatistics
{
    public class CreateDownloadTransmissionStatistics
    {
        private readonly MainDbContextFactory _contextFactory;

        public CreateDownloadTransmissionStatistics(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.Host host, Models.DownloadTransmissionStatistics statistics)
        {
            MainDbContext context = _contextFactory.Create();
            HostDto? hostDto = await context.Hosts.FindAsync(host.PhysicalAddress);

            if (hostDto == null)
            {
                throw new Exception("Host not found");
            }

            DownloadTransmissionStatisticsDto dto = new()
            {
                Host = hostDto,
                FileType = statistics.FileType,
                Size = statistics.Size,
                Time = statistics.Time
            };

            context.DownloadTransmissionStatistics.Add(dto);
            await context.SaveChangesAsync();
        }
    }
}
