using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "CryptoPayments",
                newName: "MerchantOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MerchantOrderId",
                table: "CryptoPayments",
                newName: "OrderId");
        }
    }
}
