using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class AddBestAnswerRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BestAnswerRequests",
                columns: table => new
                {
                    BestAnswerRequestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    AnswerID = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserID = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsRejected = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BestAnswerRequests", x => x.BestAnswerRequestID);
                    table.ForeignKey(
                        name: "FK_BestAnswerRequests_Answers_AnswerID",
                        column: x => x.AnswerID,
                        principalTable: "Answers",
                        principalColumn: "AnswerID");
                    table.ForeignKey(
                        name: "FK_BestAnswerRequests_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_BestAnswerRequests_Users_RequestedByUserID",
                        column: x => x.RequestedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_BestAnswerRequests_Users_ReviewedByAdminID",
                        column: x => x.ReviewedByAdminID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BestAnswerRequests_AnswerID",
                table: "BestAnswerRequests",
                column: "AnswerID");

            migrationBuilder.CreateIndex(
                name: "IX_BestAnswerRequests_PostID",
                table: "BestAnswerRequests",
                column: "PostID");

            migrationBuilder.CreateIndex(
                name: "IX_BestAnswerRequests_RequestedByUserID",
                table: "BestAnswerRequests",
                column: "RequestedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BestAnswerRequests_ReviewedByAdminID",
                table: "BestAnswerRequests",
                column: "ReviewedByAdminID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BestAnswerRequests");
        }
    }
}
