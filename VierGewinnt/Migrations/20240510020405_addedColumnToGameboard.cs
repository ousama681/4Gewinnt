using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VierGewinnt.Migrations
{
    /// <inheritdoc />
    public partial class addedColumnToGameboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "GameBoards",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "GameBoards");
        }
    }
}
