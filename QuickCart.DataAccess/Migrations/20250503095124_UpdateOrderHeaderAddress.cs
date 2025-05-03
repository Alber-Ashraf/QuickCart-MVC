using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderHeaderAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Adress",
                table: "OrderHeaders",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "OrderHeaders",
                newName: "Adress");
        }
    }
}
