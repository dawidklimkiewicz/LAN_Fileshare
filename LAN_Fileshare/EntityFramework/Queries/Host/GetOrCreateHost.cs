using LAN_Fileshare.EntityFramework.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.Host
{
    public class GetOrCreateHost
    {
        private readonly MainDbContextFactory _contextFactory;

        public GetOrCreateHost(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Models.Host?> Execute(PhysicalAddress physicalAddress, IPAddress ipAddress, string username)
        {
            using MainDbContext context = _contextFactory.Create();

            HostDto? dto = await context.Hosts.FindAsync(physicalAddress);

            if (dto == null)
            {
                Models.Host newHost = new(physicalAddress, ipAddress, username);
                dto = new HostDto
                {
                    PhysicalAddress = newHost.PhysicalAddress,
                    DownloadPath = newHost.DownloadPath,
                    IsBlocked = newHost.IsBlocked,
                    AutoDownload = newHost.AutoDownload,
                    FileUploads = new List<FileUploadDto>(),
                    DownloadTransmissionStatistics = new List<DownloadTransmissionStatisticsDto>(),
                    UploadTransmissionStatistics = new List<UploadTransmissionStatisticsDto>()
                };

                return newHost;
            }
            else
            {
                List<Models.FileUpload> uploadFiles = dto.FileUploads.Select(f => new Models.FileUpload(f.Id, f.Name, f.Path, f.Size, f.TimeCreated, f.BytesTransmitted)).ToList();

                return new Models.Host(dto.PhysicalAddress, ipAddress, username, dto.DownloadPath, dto.IsBlocked, dto.AutoDownload, uploadFiles);
            }
        }
    }
}
