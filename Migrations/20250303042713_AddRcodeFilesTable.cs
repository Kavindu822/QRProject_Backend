using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRFileTrackingapi.Migrations
{
    /// <inheritdoc />
    public partial class AddRcodeFilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RcodeFiles",
                columns: table => new
                {
                    Rcode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EpfNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QRCodeImage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RcodeFiles", x => x.Rcode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RcodeFiles");
        }
    }
}
