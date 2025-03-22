using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.UploadTransmissionStatistics
{
    public class CreateUploadTransmissionStatistics
    {
        private MainDbContextFactory _contextFactory;

        public CreateUploadTransmissionStatistics(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.Host host, Models.UploadTransmissionStatistics statistics)
        {
            MainDbContext context = _contextFactory.Create();
            HostDto? hostDto = context.Hosts.FindAsync(host.PhysicalAddress).Result;

            if (hostDto == null)
            {
                throw new Exception("Host not found");
            }

            UploadTransmissionStatisticsDto dto = new UploadTransmissionStatisticsDto
            {
                Host = hostDto,
                FileType = statistics.FileType,
                Size = statistics.Size,
                Time = statistics.Time
            };

            context.UploadTransmissionStatistics.Add(dto);
            await context.SaveChangesAsync();
        }
    }
}
