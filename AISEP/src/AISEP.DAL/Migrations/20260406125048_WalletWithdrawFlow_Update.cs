using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class WalletWithdrawFlow_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "withdraw_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WithdrawRequestId",
                table: "wallet_transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById");

            migrationBuilder.AddCheckConstraint(
                name: "CK_withdraw_requests_amount_positive",
                table: "withdraw_requests",
                sql: "\"Amount\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_WithdrawRequestId",
                table: "wallet_transactions",
                column: "WithdrawRequestId",
                unique: true,
                filter: "\"WithdrawRequestId\" IS NOT NULL AND \"Type\" = 'Withdrawal'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_wallet_transactions_amount_positive",
                table: "wallet_transactions",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions",
                column: "WithdrawRequestId",
                principalTable: "withdraw_requests",
                principalColumn: "WithdrawRequestId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropCheckConstraint(
                name: "CK_withdraw_requests_amount_positive",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_wallet_transactions_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_wallet_transactions_amount_positive",
                table: "wallet_transactions");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "WithdrawRequestId",
                table: "wallet_transactions");
        }
    }
}
