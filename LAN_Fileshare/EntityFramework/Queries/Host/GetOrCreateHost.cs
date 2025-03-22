using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
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

        public async Task<Models.Host> Execute(PhysicalAddress physicalAddress, IPAddress ipAddress, string username)
        {
            using MainDbContext context = _contextFactory.Create();
            HostDto? dto = await context.Hosts.Include(h => h.FileUploads).FirstOrDefaultAsync(h => h.PhysicalAddress.Equals(physicalAddress));

            // If host does not exist, create it and save it to the database
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

                context.Hosts.Add(dto);
                await context.SaveChangesAsync();

                return newHost;
            }
            else
            {
                // Check if the files from database still exist on the disk, if not remove them from the database
                List<Models.FileUpload> uploadFiles = new();

                if (dto.FileUploads != null)
                {
                    List<FileUploadDto> filesToCheck = dto.FileUploads.ToList() ?? new List<FileUploadDto>(); ;
                    foreach (FileUploadDto file in filesToCheck)
                    {
                        if (new FileInfo(file.Path).Exists)
                        {
                            uploadFiles.Add(new Models.FileUpload(file.Id, file.Path, file.TimeCreated, file.BytesTransmitted));
                        }
                        else
                        {
                            context.FileUploads.Remove(file);
                        }
                    }
                    await context.SaveChangesAsync();
                }
                return new Models.Host(dto.PhysicalAddress, ipAddress, username, dto.DownloadPath, dto.IsBlocked, dto.AutoDownload, uploadFiles);
            }
        }
    }
}
