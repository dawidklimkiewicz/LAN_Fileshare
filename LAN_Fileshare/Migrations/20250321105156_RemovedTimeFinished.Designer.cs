﻿// <auto-generated />
using System;
using LAN_Fileshare.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20250321105156_RemovedTimeFinished")]
    partial class RemovedTimeFinished
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.3");

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.DownloadTransmissionStatisticsDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HostPhysicalAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HostPhysicalAddress");

                    b.ToTable("DownloadTransmissionStatistics");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.FileDownloadDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("BytesTransmitted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HostPhysicalAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeCreated")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HostPhysicalAddress");

                    b.ToTable("FileDownloads");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.FileUploadDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("BytesTransmitted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("HostPhysicalAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TimeCreated")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HostPhysicalAddress");

                    b.ToTable("FileUploads");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.HostDto", b =>
                {
                    b.Property<string>("PhysicalAddress")
                        .HasColumnType("TEXT");

                    b.Property<bool>("AutoDownload")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DownloadPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBlocked")
                        .HasColumnType("INTEGER");

                    b.HasKey("PhysicalAddress");

                    b.ToTable("Hosts");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.SettingsDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AutoDownloadDefault")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CopyUploadedFileToWorkspace")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CopyUploadedFileToWorkspaceMaxSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DefaultDownloadPath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.UploadTransmissionStatisticsDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HostPhysicalAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Size")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("HostPhysicalAddress");

                    b.ToTable("UploadTransmissionStatistics");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.DownloadTransmissionStatisticsDto", b =>
                {
                    b.HasOne("LAN_Fileshare.EntityFramework.DTOs.HostDto", "Host")
                        .WithMany("DownloadTransmissionStatistics")
                        .HasForeignKey("HostPhysicalAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Host");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.FileDownloadDto", b =>
                {
                    b.HasOne("LAN_Fileshare.EntityFramework.DTOs.HostDto", "Host")
                        .WithMany("FileDownloads")
                        .HasForeignKey("HostPhysicalAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Host");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.FileUploadDto", b =>
                {
                    b.HasOne("LAN_Fileshare.EntityFramework.DTOs.HostDto", "Host")
                        .WithMany("FileUploads")
                        .HasForeignKey("HostPhysicalAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Host");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.UploadTransmissionStatisticsDto", b =>
                {
                    b.HasOne("LAN_Fileshare.EntityFramework.DTOs.HostDto", "Host")
                        .WithMany("UploadTransmissionStatistics")
                        .HasForeignKey("HostPhysicalAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Host");
                });

            modelBuilder.Entity("LAN_Fileshare.EntityFramework.DTOs.HostDto", b =>
                {
                    b.Navigation("DownloadTransmissionStatistics");

                    b.Navigation("FileDownloads");

                    b.Navigation("FileUploads");

                    b.Navigation("UploadTransmissionStatistics");
                });
#pragma warning restore 612, 618
        }
    }
}
