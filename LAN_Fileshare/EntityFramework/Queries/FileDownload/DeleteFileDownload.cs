using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileDownload
{
    public class DeleteFileDownload
    {
        private readonly MainDbContextFactory _contextFactory;

        public DeleteFileDownload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Guid Id)
        {
            using MainDbContext context = _contextFactory.Create();
            FileDownloadDto dto = new() { Id = Id };
            context.FileDownloads.Remove(dto);
            await context.SaveChangesAsync();
        }
    }
}
