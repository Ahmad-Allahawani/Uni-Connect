using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationActorUserID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorUserID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorUserID",
                table: "Notifications",
                column: "ActorUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_ActorUserID",
                table: "Notifications",
                column: "ActorUserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_ActorUserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ActorUserID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActorUserID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Messages");
        }
    }
}
