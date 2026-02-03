using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    MerchantOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
