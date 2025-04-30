using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuickCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InsertCompaniesIntoDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "City", "Name", "Phone", "PostalCode", "State" },
                values: new object[,]
                {
                    { 1, "123 Main St", "New York", "Tech group", "123-456-7890", "10001", "NY" },
                    { 2, "456 Elm St", "Los Angeles", "Tech group 2", "987-654-3210", "90001", "CA" },
                    { 3, "789 Oak St", "Chicago", "Tech group 3", "555-555-5555", "60601", "IL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
