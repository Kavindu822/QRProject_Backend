using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class AddContactNoENameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "AspNetUsers",
                newName: "ContactNo");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "EName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EName",
                table: "AspNetUsers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ContactNo",
                table: "AspNetUsers",
                newName: "Phone");
        }
    }
}
