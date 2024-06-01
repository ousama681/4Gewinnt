using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VierGewinnt.Migrations
{
    /// <inheritdoc />
    public partial class addedPlaerRankingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerRankings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRankings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlayerRankings_AspNetUsers_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRankings_PlayerID",
                table: "PlayerRankings",
                column: "PlayerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRankings");
        }
    }
}
