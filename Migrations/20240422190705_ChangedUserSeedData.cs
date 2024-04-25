using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VierGewinnt.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUserSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "1", 0, "dc738904-5800-48fe-84da-0c182e1d1765", "abc@abc.com", false, false, null, null, null, "passwort123", null, false, "6b87ea83-9695-4d69-9bee-276870e3ad30", false, "TheLegend27" },
                    { "2", 0, "06715f6a-f5e8-4c0f-b10b-08e29f0d1384", "bobo@abc.com", false, false, null, null, null, "wertwert", null, false, "a99deb1b-26db-4b25-9164-dec641d13cbb", false, "DjBobo1337" },
                    { "3", 0, "fa2574f3-10db-4c7f-9430-85aaffdc36ee", "Frodo@abc.com", false, false, null, null, null, "qwert789", null, false, "a5f6f1b1-26cc-42ec-a308-32303be76669", false, "FBeutlin69" },
                    { "4", 0, "1c5507ad-5ad2-4625-8ca2-877f83e6bd2b", "Frodo@abc.com", false, false, null, null, null, "afasfwafafa", null, false, "c3324e99-865b-46d5-bc87-4735d3b11783", false, "Son_Goku" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "4");
        }
    }
}
