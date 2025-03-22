using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LAN_Fileshare.EntityFramework
{
    public class MainDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext(string[] args)
        {
            return new MainDbContext(new DbContextOptionsBuilder<MainDbContext>().UseSqlite("Data Source=database.db").Options);
        }
    }
}
