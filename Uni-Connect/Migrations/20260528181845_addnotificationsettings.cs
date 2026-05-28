using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class addnotificationsettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnAnswers",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnSessionRequests",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyOnAnswers",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotifyOnSessionRequests",
                table: "Users");
        }
    }
}
