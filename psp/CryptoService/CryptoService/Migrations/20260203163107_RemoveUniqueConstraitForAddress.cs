using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraitForAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CryptoPayments_BitcoinAddress",
                table: "CryptoPayments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CryptoPayments_BitcoinAddress",
                table: "CryptoPayments",
                column: "BitcoinAddress",
                unique: true);
        }
    }
}
