using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileUpload
{
    public class CreateFileUpload
    {
        private readonly MainDbContextFactory _contextFactory;

        public CreateFileUpload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.Host host, Models.FileUpload fileUpload)
        {
            MainDbContext context = _contextFactory.Create();

            HostDto? hostDto = await context.Hosts.FindAsync(host.PhysicalAddress);
            if (hostDto == null)
            {
                throw new Exception("Host not found");
            }

            FileUploadDto fileUploadDto = new()
            {
                Id = fileUpload.Id,
                Host = hostDto,
                Name = fileUpload.Name,
                Path = fileUpload.Path,
                Size = fileUpload.Size,
                BytesTransmitted = fileUpload.BytesTransmitted,
                TimeCreated = fileUpload.TimeCreated,
            };

            context.FileUploads.Add(fileUploadDto);
            await context.SaveChangesAsync();
        }
    }
}
