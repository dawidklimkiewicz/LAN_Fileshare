using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.Host
{
    public class UpdateHost
    {
        private MainDbContextFactory _contextFactory;

        public UpdateHost(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.Host host)
        {
            MainDbContext context = _contextFactory.Create();
            HostDto? hostToUpdate = await context.Hosts.FindAsync(host.PhysicalAddress);

            if (hostToUpdate == null)
            {
                throw new Exception("Host not found");
            }

            hostToUpdate.DownloadPath = host.DownloadPath;
            hostToUpdate.IsBlocked = host.IsBlocked;
            hostToUpdate.AutoDownload = host.AutoDownload;

            context.Hosts.Update(hostToUpdate);
            await context.SaveChangesAsync();
        }
    }
}
