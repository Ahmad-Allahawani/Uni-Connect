using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class AddPostViewsAndSessionRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "PrivateSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "PrivateSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostViews",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PostID = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostViews", x => new { x.UserID, x.PostID });
                    table.ForeignKey(
                        name: "FK_PostViews_Posts_PostID",
                        column: x => x.PostID,
                        principalTable: "Posts",
                        principalColumn: "PostID");
                    table.ForeignKey(
                        name: "FK_PostViews_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostViews_PostID",
                table: "PostViews",
                column: "PostID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostViews");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "PrivateSessions");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "PrivateSessions");
        }
    }
}
