using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Collections.Generic;
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

        public async Task Execute(Models.Host host, List<Models.FileUpload> fileUpload)
        {
            MainDbContext context = _contextFactory.Create();

            HostDto? hostDto = await context.Hosts.FindAsync(host.PhysicalAddress);
            if (hostDto == null)
            {
                throw new Exception("Host not found");
            }

            List<FileUploadDto> fileUploadDtos = new();

            foreach (Models.FileUpload file in fileUpload)
            {
                FileUploadDto fileUploadDto = new()
                {
                    Id = file.Id,
                    Host = hostDto,
                    Path = file.Path,
                    BytesTransmitted = file.BytesTransmitted,
                    TimeCreated = file.TimeCreated,
                };
                fileUploadDtos.Add(fileUploadDto);
            }

            context.FileUploads.AddRange(fileUploadDtos);
            await context.SaveChangesAsync();
        }
    }
}
