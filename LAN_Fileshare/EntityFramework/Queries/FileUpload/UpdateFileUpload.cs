using LAN_Fileshare.EntityFramework.DTOs;
using System;
using System.Threading.Tasks;

namespace LAN_Fileshare.EntityFramework.Queries.FileUpload
{
    public class UpdateFileUpload
    {
        private readonly MainDbContextFactory _contextFactory;

        public UpdateFileUpload(MainDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(Models.FileUpload fileUpload)
        {
            MainDbContext context = _contextFactory.Create();
            FileUploadDto? dto = await context.FileUploads.FindAsync(fileUpload.Id);

            if (dto == null)
            {
                throw new InvalidOperationException("File upload not found.");
            }

            dto.Path = fileUpload.Path;
            dto.BytesTransmitted = fileUpload.BytesTransmitted;
            dto.TimeFinished = fileUpload.TimeFinished;

            context.FileUploads.Update(dto);
            await context.SaveChangesAsync();
        }
    }
}
