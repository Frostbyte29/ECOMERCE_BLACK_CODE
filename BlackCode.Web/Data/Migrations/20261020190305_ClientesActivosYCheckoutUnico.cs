using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlackCode.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClientesActivosYCheckoutUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutToken",
                table: "Orders",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CheckoutToken",
                table: "Orders",
                column: "CheckoutToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CheckoutToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CheckoutToken",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
