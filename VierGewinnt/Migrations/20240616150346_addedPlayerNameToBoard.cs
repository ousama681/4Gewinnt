using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VierGewinnt.Migrations
{
    /// <inheritdoc />
    public partial class addedPlayerNameToBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayerOneName",
                table: "GameBoards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlayerTwoName",
                table: "GameBoards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerOneName",
                table: "GameBoards");

            migrationBuilder.DropColumn(
                name: "PlayerTwoName",
                table: "GameBoards");
        }
    }
}
