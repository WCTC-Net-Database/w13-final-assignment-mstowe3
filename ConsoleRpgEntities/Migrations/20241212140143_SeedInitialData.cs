using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleRpgEntities.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Description", "EastId", "MonsterId", "Name", "NorthId", "PlayerId", "SouthId", "WestId" },
                values: new object[] { 1, "This is the starter room.", null, null, "Starter Room", null, null, null, null });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "EquipmentId", "Experience", "Health", "Name", "RoomId" },
                values: new object[] { 1, null, 0, 100, "Hero", 1 });

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "Id", "EquipmentId", "Experience", "Health", "Name", "RoomId" },
                values: new object[] { 2, null, 20, 80, "Mage", 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
