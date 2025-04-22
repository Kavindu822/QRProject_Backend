using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class AddRcodeFileHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RcodeFilesHistory",
                table: "RcodeFilesHistory");

            migrationBuilder.RenameTable(
                name: "RcodeFilesHistory",
                newName: "RcodeFilesHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RcodeFilesHistories",
                table: "RcodeFilesHistories",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RcodeFilesHistories",
                table: "RcodeFilesHistories");

            migrationBuilder.RenameTable(
                name: "RcodeFilesHistories",
                newName: "RcodeFilesHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RcodeFilesHistory",
                table: "RcodeFilesHistory",
                column: "Id");
        }
    }
}
