using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    public partial class LinkWalletTransactionWithWithdrawRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WithdrawRequestId",
                table: "wallet_transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_wallet_transactions_amount_positive",
                table: "wallet_transactions",
                sql: "\"Amount\" > 0");

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

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions",
                column: "WithdrawRequestId",
                principalTable: "withdraw_requests",
                principalColumn: "WithdrawRequestId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropIndex(
                name: "IX_wallet_transactions_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_wallet_transactions_amount_positive",
                table: "wallet_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_withdraw_requests_amount_positive",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "WithdrawRequestId",
                table: "wallet_transactions");
        }
    }
}
