using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileDownload
{
    public class GetAllFileDownload
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetAllFileDownload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Models.FileDownload>> Execute(Models.Host host)
        {
            using MainDbContext context = _contextFactory.Create();
            List<FileDownloadDto> dtos = await context.Hosts
                .Where(h => h.PhysicalAddress.Equals(host.PhysicalAddress))
                .SelectMany(h => h.FileDownloads)
                .ToListAsync();

            return dtos.Select(f => new Models.FileDownload(f.Id, f.Name, f.Size, f.TimeCreated, f.TimeFinished, f.BytesTransmitted)).ToList();
        }
    }
}
