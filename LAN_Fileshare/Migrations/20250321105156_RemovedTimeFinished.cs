using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTimeFinished : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeFinished",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "TimeFinished",
                table: "FileDownloads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeFinished",
                table: "FileUploads",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeFinished",
                table: "FileDownloads",
                type: "TEXT",
                nullable: true);
        }
    }
}
