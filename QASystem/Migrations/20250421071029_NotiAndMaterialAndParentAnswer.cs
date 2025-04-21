using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QASystem.Migrations
{
    /// <inheritdoc />
    public partial class NotiAndMaterialAndParentAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentAnswerID",
                table: "Answers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Downloads = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Materials__C506131A9F3E2C1D", x => x.MaterialID);
                    table.ForeignKey(
                        name: "FK__Materials__UserID__4A8310C6",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionID = table.Column<int>(type: "int", nullable: true),
                    AnswerID = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifications__20CF2E32B8A2F7E4", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK__Notifications__AnswerID__4E53A1AA",
                        column: x => x.AnswerID,
                        principalTable: "Answers",
                        principalColumn: "AnswerID");
                    table.ForeignKey(
                        name: "FK__Notifications__QuestionID__4D5F7D71",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID");
                    table.ForeignKey(
                        name: "FK__Notifications__UserID__4C6B5938",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_ParentAnswerID",
                table: "Answers",
                column: "ParentAnswerID");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_UserID",
                table: "Materials",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AnswerID",
                table: "Notifications",
                column: "AnswerID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_QuestionID",
                table: "Notifications",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK__Answers__ParentAnswerID__31EC6D26",
                table: "Answers",
                column: "ParentAnswerID",
                principalTable: "Answers",
                principalColumn: "AnswerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Answers__ParentAnswerID__31EC6D26",
                table: "Answers");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Answers_ParentAnswerID",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "ParentAnswerID",
                table: "Answers");
        }
    }
}
