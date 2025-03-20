using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileUpload
{
    public class DeleteFileUpload
    {
        private MainDbContextFactory _contextFactory;

        public DeleteFileUpload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Guid id)
        {
            using MainDbContext context = _contextFactory.Create();
            FileUploadDto dto = new() { Id = id };
            context.FileUploads.Remove(dto);
            await context.SaveChangesAsync();
        }
    }
}
