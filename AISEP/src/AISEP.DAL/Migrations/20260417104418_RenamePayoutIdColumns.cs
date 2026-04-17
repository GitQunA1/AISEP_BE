using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenamePayoutIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_payout_groups_MonthlyPayoutBatchId",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_payouts_MonthlyPayoutId",
                table: "wallet_transactions");

            migrationBuilder.RenameColumn(
                name: "MonthlyPayoutId",
                table: "wallet_transactions",
                newName: "PayoutId");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transactions_MonthlyPayoutId",
                table: "wallet_transactions",
                newName: "IX_wallet_transactions_PayoutId");

            migrationBuilder.RenameColumn(
                name: "MonthlyPayoutBatchId",
                table: "payouts",
                newName: "PayoutGroupId");

            migrationBuilder.RenameColumn(
                name: "MonthlyPayoutId",
                table: "payouts",
                newName: "PayoutId");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_MonthlyPayoutBatchId",
                table: "payouts",
                newName: "IX_payouts_PayoutGroupId");

            migrationBuilder.RenameColumn(
                name: "MonthlyPayoutBatchId",
                table: "payout_groups",
                newName: "PayoutGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_payout_groups_PayoutGroupId",
                table: "payouts",
                column: "PayoutGroupId",
                principalTable: "payout_groups",
                principalColumn: "PayoutGroupId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_payouts_PayoutId",
                table: "wallet_transactions",
                column: "PayoutId",
                principalTable: "payouts",
                principalColumn: "PayoutId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_payout_groups_PayoutGroupId",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_payouts_PayoutId",
                table: "wallet_transactions");

            migrationBuilder.RenameColumn(
                name: "PayoutId",
                table: "wallet_transactions",
                newName: "MonthlyPayoutId");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transactions_PayoutId",
                table: "wallet_transactions",
                newName: "IX_wallet_transactions_MonthlyPayoutId");

            migrationBuilder.RenameColumn(
                name: "PayoutGroupId",
                table: "payouts",
                newName: "MonthlyPayoutBatchId");

            migrationBuilder.RenameColumn(
                name: "PayoutId",
                table: "payouts",
                newName: "MonthlyPayoutId");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_PayoutGroupId",
                table: "payouts",
                newName: "IX_payouts_MonthlyPayoutBatchId");

            migrationBuilder.RenameColumn(
                name: "PayoutGroupId",
                table: "payout_groups",
                newName: "MonthlyPayoutBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_payout_groups_MonthlyPayoutBatchId",
                table: "payouts",
                column: "MonthlyPayoutBatchId",
                principalTable: "payout_groups",
                principalColumn: "MonthlyPayoutBatchId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_payouts_MonthlyPayoutId",
                table: "wallet_transactions",
                column: "MonthlyPayoutId",
                principalTable: "payouts",
                principalColumn: "MonthlyPayoutId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
