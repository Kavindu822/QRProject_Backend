using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the 'IsDeleted' column from 'AspNetUsers'
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            // Alter 'Status' column in 'RcodeFiles' table to set max length to 20
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RcodeFiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Alter 'EpfNo' column in 'RcodeFiles' table to set max length to 20
            migrationBuilder.AlterColumn<string>(
                name: "EpfNo",
                table: "RcodeFiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Alter 'EName' column in 'RcodeFiles' table to set max length to 100
            migrationBuilder.AlterColumn<string>(
                name: "EName",
                table: "RcodeFiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Alter 'ContactNo' column in 'RcodeFiles' table to set max length to 15
            migrationBuilder.AlterColumn<string>(
                name: "ContactNo",
                table: "RcodeFiles",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Alter 'EpfNo' column in 'AspNetUsers' table to set max length to 450
            migrationBuilder.AlterColumn<string>(
                name: "EpfNo",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback changes made in the 'Up' method

            // Alter 'Status' column in 'RcodeFiles' back to original type
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RcodeFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // Alter 'EpfNo' column in 'RcodeFiles' back to original type
            migrationBuilder.AlterColumn<string>(
                name: "EpfNo",
                table: "RcodeFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // Alter 'EName' column in 'RcodeFiles' back to original type
            migrationBuilder.AlterColumn<string>(
                name: "EName",
                table: "RcodeFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            // Alter 'ContactNo' column in 'RcodeFiles' back to original type
            migrationBuilder.AlterColumn<string>(
                name: "ContactNo",
                table: "RcodeFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            // Alter 'EpfNo' column in 'AspNetUsers' back to original type
            migrationBuilder.AlterColumn<string>(
                name: "EpfNo",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Add the 'IsDeleted' column back to 'AspNetUsers'
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
