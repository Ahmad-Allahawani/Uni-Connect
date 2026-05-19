using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class AddVoteTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerVotes",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AnswerID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerVotes", x => new { x.UserID, x.AnswerID });
                    table.ForeignKey(
                        name: "FK_AnswerVotes_Answers_AnswerID",
                        column: x => x.AnswerID,
                        principalTable: "Answers",
                        principalColumn: "AnswerID");
                    table.ForeignKey(
                        name: "FK_AnswerVotes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "PostVotes",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostVotes", x => new { x.UserID, x.PostID });
                    table.ForeignKey(
                        name: "FK_PostVotes_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_PostVotes_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerVotes_AnswerID",
                table: "AnswerVotes",
                column: "AnswerID");

            migrationBuilder.CreateIndex(
                name: "IX_PostVotes_PostID",
                table: "PostVotes",
                column: "PostID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerVotes");

            migrationBuilder.DropTable(
                name: "PostVotes");
        }
    }
}
