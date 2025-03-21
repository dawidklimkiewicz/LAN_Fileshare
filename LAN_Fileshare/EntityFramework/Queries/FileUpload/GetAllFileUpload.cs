using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileUpload
{
    public class GetAllFileUpload
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetAllFileUpload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Models.FileUpload>> Execute(Models.Host host)
        {
            using MainDbContext context = _contextFactory.Create();
            List<FileUploadDto> dtos = await context.Hosts
                .Where(h => h.PhysicalAddress.Equals(host.PhysicalAddress))
                .SelectMany(h => h.FileUploads)
                .ToListAsync();

            return dtos.Select(f => new Models.FileUpload(f.Id, f.Name, f.Path, f.Size, f.TimeCreated, f.BytesTransmitted)).ToList();
        }
    }
}
