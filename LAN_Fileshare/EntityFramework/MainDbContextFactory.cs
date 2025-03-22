using Microsoft.EntityFrameworkCore;

namespace LAN_Fileshare.EntityFramework
{
    public class MainDbContextFactory
    {
        private readonly DbContextOptions _options;

        public MainDbContextFactory(DbContextOptions options)
        {
            _options = options;
        }

        public MainDbContext Create()
        {
            return new MainDbContext(_options);
        }
    }
}
