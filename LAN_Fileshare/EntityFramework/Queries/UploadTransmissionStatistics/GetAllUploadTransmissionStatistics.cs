using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.UploadTransmissionStatistics
{
    public class GetAllUploadTransmissionStatistics
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetAllUploadTransmissionStatistics(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Models.UploadTransmissionStatistics>> Execute(Models.Host host)
        {
            using MainDbContext context = _contextFactory.Create();

            List<UploadTransmissionStatisticsDto> dtos = await context.Hosts
                .Where(h => h.PhysicalAddress.Equals(host.PhysicalAddress))
                .SelectMany(h => h.UploadTransmissionStatistics)
                .ToListAsync();

            return dtos.Select(stat => new Models.UploadTransmissionStatistics(stat.FileType, stat.Size, stat.Time)).ToList();
        }
    }
}
