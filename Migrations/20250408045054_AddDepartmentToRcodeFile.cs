using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToRcodeFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "RcodeFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "RcodeFiles");
        }
    }
}
