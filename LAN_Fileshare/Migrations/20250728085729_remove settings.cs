using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LAN_Fileshare.Migrations
{
    /// <inheritdoc />
    public partial class removesettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutoDownloadDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CopyUploadedFileToWorkspace = table.Column<bool>(type: "INTEGER", nullable: false),
                    CopyUploadedFileToWorkspaceMaxSize = table.Column<long>(type: "INTEGER", nullable: false),
                    DefaultDownloadPath = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }
    }
}
