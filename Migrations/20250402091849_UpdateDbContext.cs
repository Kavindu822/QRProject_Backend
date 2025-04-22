using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GetDate",
                table: "RcodeFilesHistory");

            migrationBuilder.RenameColumn(
                name: "UpdateDate",
                table: "RcodeFilesHistory",
                newName: "TransferDate");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "RcodeFilesHistory",
                newName: "PreviousEpfNo");

            migrationBuilder.RenameColumn(
                name: "EpfNo",
                table: "RcodeFilesHistory",
                newName: "PreviousEName");

            migrationBuilder.RenameColumn(
                name: "EName",
                table: "RcodeFilesHistory",
                newName: "PreviousContactNo");

            migrationBuilder.RenameColumn(
                name: "ContactNo",
                table: "RcodeFilesHistory",
                newName: "NewEpfNo");

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "RcodeFilesHistory",
                newName: "NewEName");

            migrationBuilder.RenameColumn(
                name: "HistoryId",
                table: "RcodeFilesHistory",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "NewContactNo",
                table: "RcodeFilesHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewContactNo",
                table: "RcodeFilesHistory");

            migrationBuilder.RenameColumn(
                name: "TransferDate",
                table: "RcodeFilesHistory",
                newName: "UpdateDate");

            migrationBuilder.RenameColumn(
                name: "PreviousEpfNo",
                table: "RcodeFilesHistory",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "PreviousEName",
                table: "RcodeFilesHistory",
                newName: "EpfNo");

            migrationBuilder.RenameColumn(
                name: "PreviousContactNo",
                table: "RcodeFilesHistory",
                newName: "EName");

            migrationBuilder.RenameColumn(
                name: "NewEpfNo",
                table: "RcodeFilesHistory",
                newName: "ContactNo");

            migrationBuilder.RenameColumn(
                name: "NewEName",
                table: "RcodeFilesHistory",
                newName: "ActionType");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RcodeFilesHistory",
                newName: "HistoryId");

            migrationBuilder.AddColumn<DateTime>(
                name: "GetDate",
                table: "RcodeFilesHistory",
                type: "datetime2",
                nullable: true);
        }
    }
}
