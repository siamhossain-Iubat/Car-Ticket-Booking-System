using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarTicketBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedTravelRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TravelRoutes",
                columns: new[] { "Id", "Fare", "From", "To", "TravelTime" },
                values: new object[,]
                {
                    { 1, 800m, "Dhaka", "Chattogram", "8h" },
                    { 2, 700m, "Dhaka", "Sylhet", "6h" },
                    { 3, 600m, "Dhaka", "Rajshahi", "7h" },
                    { 4, 650m, "Dhaka", "Khulna", "7h" },
                    { 5, 550m, "Dhaka", "Barishal", "6h" },
                    { 6, 400m, "Chattogram", "Cox's Bazar", "4h" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TravelRoutes",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
