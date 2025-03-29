using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QASystem.Migrations
{
    /// <inheritdoc />
    public partial class AddreportsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportDate",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ReportReason",
                table: "Reports",
                newName: "Reason");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportedAt",
                table: "Reports",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportedAt",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Reports",
                newName: "ReportReason");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportDate",
                table: "Reports",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())");
        }
    }
}
