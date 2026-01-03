using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SepWebshop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatCarsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InsuranceId",
                table: "Orders",
                column: "InsuranceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Insurances_InsuranceId",
                table: "Orders",
                column: "InsuranceId",
                principalTable: "Insurances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Insurances_InsuranceId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_InsuranceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InsuranceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Orders");
        }
    }
}
