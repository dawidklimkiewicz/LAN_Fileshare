using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    /// <inheritdoc />
    public partial class RemovedNameAndSizeFromFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "FileUploads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FileUploads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "FileUploads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
