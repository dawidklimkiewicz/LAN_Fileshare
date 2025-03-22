using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hosts",
                columns: table => new
                {
                    PhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadPath = table.Column<string>(type: "TEXT", nullable: false),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoDownload = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.PhysicalAddress);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultDownloadPath = table.Column<string>(type: "TEXT", nullable: false),
                    AutoDownloadDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CopyUploadedFileToWorkspace = table.Column<bool>(type: "INTEGER", nullable: false),
                    CopyUploadedFileToWorkspaceMaxSize = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DownloadTransmissionStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HostPhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadTransmissionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadTransmissionStatistics_Hosts_HostPhysicalAddress",
                        column: x => x.HostPhysicalAddress,
                        principalTable: "Hosts",
                        principalColumn: "PhysicalAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileDownloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostPhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    BytesTransmitted = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeFinished = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDownloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileDownloads_Hosts_HostPhysicalAddress",
                        column: x => x.HostPhysicalAddress,
                        principalTable: "Hosts",
                        principalColumn: "PhysicalAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostPhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    BytesTransmitted = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeFinished = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileUploads_Hosts_HostPhysicalAddress",
                        column: x => x.HostPhysicalAddress,
                        principalTable: "Hosts",
                        principalColumn: "PhysicalAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadTransmissionStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HostPhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadTransmissionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadTransmissionStatistics_Hosts_HostPhysicalAddress",
                        column: x => x.HostPhysicalAddress,
                        principalTable: "Hosts",
                        principalColumn: "PhysicalAddress",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadTransmissionStatistics_HostPhysicalAddress",
                table: "DownloadTransmissionStatistics",
                column: "HostPhysicalAddress");

            migrationBuilder.CreateIndex(
                name: "IX_FileDownloads_HostPhysicalAddress",
                table: "FileDownloads",
                column: "HostPhysicalAddress");

            migrationBuilder.CreateIndex(
                name: "IX_FileUploads_HostPhysicalAddress",
                table: "FileUploads",
                column: "HostPhysicalAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UploadTransmissionStatistics_HostPhysicalAddress",
                table: "UploadTransmissionStatistics",
                column: "HostPhysicalAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadTransmissionStatistics");

            migrationBuilder.DropTable(
                name: "FileDownloads");

            migrationBuilder.DropTable(
                name: "FileUploads");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UploadTransmissionStatistics");

            migrationBuilder.DropTable(
                name: "Hosts");
        }
    }
}
