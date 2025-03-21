using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFileDownload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileDownloads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileDownloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HostPhysicalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    BytesTransmitted = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_FileDownloads_HostPhysicalAddress",
                table: "FileDownloads",
                column: "HostPhysicalAddress");
        }
    }
}
