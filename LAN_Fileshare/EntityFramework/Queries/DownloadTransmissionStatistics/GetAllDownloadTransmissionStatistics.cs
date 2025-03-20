using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.DownloadTransmissionStatistics
{
    public class GetAllDownloadTransmissionStatistics
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetAllDownloadTransmissionStatistics(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Models.DownloadTransmissionStatistics>> Execute(Models.Host host)
        {
            using MainDbContext context = _contextFactory.Create();
            List<DownloadTransmissionStatisticsDto> dtos = await context.Hosts
                .Where(h => h.PhysicalAddress.Equals(host.PhysicalAddress))
                .SelectMany(h => h.DownloadTransmissionStatistics)
                .ToListAsync();

            return dtos.Select(stat => new Models.DownloadTransmissionStatistics(stat.FileType, stat.Size, stat.Time)).ToList();
        }
    }
}
