using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMerchantIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint from PaymentRequests to Merchants
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Merchants_MerchantId",
                table: "PaymentRequests");

            // Drop the foreign key constraint from Merchants to Accounts
            migrationBuilder.DropForeignKey(
                name: "FK_Merchants_Accounts_AccountId",
                table: "Merchants");

            // Drop the index on MerchantId in PaymentRequests
            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_MerchantId",
                table: "PaymentRequests");

            // Drop the unique index on AccountId in Merchants
            migrationBuilder.DropIndex(
                name: "IX_Merchants_AccountId",
                table: "Merchants");

            // Drop the primary key constraint on Merchants table
            migrationBuilder.DropPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants");

            // Change the data type of MerchantId in PaymentRequests table
            migrationBuilder.AlterColumn<string>(
                name: "MerchantId",
                table: "PaymentRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Change the data type of Id in Merchants table
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Merchants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Recreate the primary key constraint on Merchants table
            migrationBuilder.AddPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants",
                column: "Id");

            // Recreate the unique index on AccountId in Merchants
            migrationBuilder.CreateIndex(
                name: "IX_Merchants_AccountId",
                table: "Merchants",
                column: "AccountId",
                unique: true);

            // Recreate the index on MerchantId in PaymentRequests
            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_MerchantId",
                table: "PaymentRequests",
                column: "MerchantId");

            // Recreate the foreign key constraint from Merchants to Accounts
            migrationBuilder.AddForeignKey(
                name: "FK_Merchants_Accounts_AccountId",
                table: "Merchants",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction); // Note: Was CASCADE in original?

            // Recreate the foreign key constraint from PaymentRequests to Merchants
            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Merchants_MerchantId",
                table: "PaymentRequests",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all constraints in reverse order
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Merchants_MerchantId",
                table: "PaymentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Merchants_Accounts_AccountId",
                table: "Merchants");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_MerchantId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_Merchants_AccountId",
                table: "Merchants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants");

            // Revert columns to Guid
            migrationBuilder.AlterColumn<Guid>(
                name: "MerchantId",
                table: "PaymentRequests",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Merchants",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Recreate everything in original state
            migrationBuilder.AddPrimaryKey(
                name: "PK_Merchants",
                table: "Merchants",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_AccountId",
                table: "Merchants",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_MerchantId",
                table: "PaymentRequests",
                column: "MerchantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Merchants_Accounts_AccountId",
                table: "Merchants",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Merchants_MerchantId",
                table: "PaymentRequests",
                column: "MerchantId",
                principalTable: "Merchants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}