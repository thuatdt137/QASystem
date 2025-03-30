using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QASystem.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameToDisabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Answers",
                newName: "IsDisabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDisabled",
                table: "Answers",
                newName: "IsDisable");
        }
    }
}
