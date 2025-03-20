using LAN_Fileshare.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LAN_Fileshare.EntityFramework
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<SettingsDto> Settings { get; set; }
        public DbSet<HostDto> Hosts { get; set; }
        public DbSet<FileUploadDto> FileUploads { get; set; }
        public DbSet<FileDownloadDto> FileDownloads { get; set; }
        public DbSet<DownloadTransmissionStatisticsDto> DownloadTransmissionStatistics { get; set; }
        public DbSet<UploadTransmissionStatisticsDto> UploadTransmissionStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileUploadDto>().HasOne(f => f.Host).WithMany(h => h.FileUploads).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FileDownloadDto>().HasOne(f => f.Host).WithMany(h => h.FileDownloads).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DownloadTransmissionStatisticsDto>().HasOne(f => f.Host).WithMany(h => h.DownloadTransmissionStatistics).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UploadTransmissionStatisticsDto>().HasOne(f => f.Host).WithMany(h => h.UploadTransmissionStatistics).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
