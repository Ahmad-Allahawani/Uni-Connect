using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uni_Connect.Migrations
{
    /// <inheritdoc />
    public partial class addanswwertoreport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnswerID",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AnswerID",
                table: "Reports",
                column: "AnswerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Answers_AnswerID",
                table: "Reports",
                column: "AnswerID",
                principalTable: "Answers",
                principalColumn: "AnswerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Answers_AnswerID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AnswerID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "AnswerID",
                table: "Reports");
        }
    }
}
