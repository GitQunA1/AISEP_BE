using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameMonthlyPayoutTablesToPayouts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_monthly_payout_batches_MonthlyPayoutBatchId",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_ApprovedById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_PaidById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RejectedById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RetryRequestedById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RetryReviewedById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_wallets_WalletId",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_monthly_payouts_MonthlyPayoutId",
                table: "wallet_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_monthly_payouts",
                table: "monthly_payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payouts_amount_positive",
                table: "monthly_payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payouts_period_range",
                table: "monthly_payouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_monthly_payout_batches",
                table: "monthly_payout_batches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payout_batches_date_range",
                table: "monthly_payout_batches");

            migrationBuilder.RenameTable(
                name: "monthly_payouts",
                newName: "payouts");

            migrationBuilder.RenameTable(
                name: "monthly_payout_batches",
                newName: "payout_groups");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_WalletId",
                table: "payouts",
                newName: "IX_payouts_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_RetryReviewedById",
                table: "payouts",
                newName: "IX_payouts_RetryReviewedById");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_RetryRequestedById",
                table: "payouts",
                newName: "IX_payouts_RetryRequestedById");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_RejectedById",
                table: "payouts",
                newName: "IX_payouts_RejectedById");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_PeriodFromDate_PeriodToDate_Status",
                table: "payouts",
                newName: "IX_payouts_PeriodFromDate_PeriodToDate_Status");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_PaidById",
                table: "payouts",
                newName: "IX_payouts_PaidById");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_MonthlyPayoutBatchId",
                table: "payouts",
                newName: "IX_payouts_MonthlyPayoutBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payouts_ApprovedById",
                table: "payouts",
                newName: "IX_payouts_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payout_batches_Status",
                table: "payout_groups",
                newName: "IX_payout_groups_Status");

            migrationBuilder.RenameIndex(
                name: "IX_monthly_payout_batches_FromDate_ToDate",
                table: "payout_groups",
                newName: "IX_payout_groups_FromDate_ToDate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payouts",
                table: "payouts",
                column: "MonthlyPayoutId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_payout_groups",
                table: "payout_groups",
                column: "MonthlyPayoutBatchId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payouts_amount_positive",
                table: "payouts",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payouts_period_range",
                table: "payouts",
                sql: "\"PeriodFromDate\" <= \"PeriodToDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payout_groups_date_range",
                table: "payout_groups",
                sql: "\"FromDate\" <= \"ToDate\"");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_payout_groups_MonthlyPayoutBatchId",
                table: "payouts",
                column: "MonthlyPayoutBatchId",
                principalTable: "payout_groups",
                principalColumn: "MonthlyPayoutBatchId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_ApprovedById",
                table: "payouts",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_PaidById",
                table: "payouts",
                column: "PaidById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_RejectedById",
                table: "payouts",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_RetryRequestedById",
                table: "payouts",
                column: "RetryRequestedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_RetryReviewedById",
                table: "payouts",
                column: "RetryReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_wallets_WalletId",
                table: "payouts",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_payouts_MonthlyPayoutId",
                table: "wallet_transactions",
                column: "MonthlyPayoutId",
                principalTable: "payouts",
                principalColumn: "MonthlyPayoutId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_payout_groups_MonthlyPayoutBatchId",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_ApprovedById",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_PaidById",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_RejectedById",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_RetryRequestedById",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_RetryReviewedById",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_payouts_wallets_WalletId",
                table: "payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_payouts_MonthlyPayoutId",
                table: "wallet_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payouts",
                table: "payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payouts_amount_positive",
                table: "payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payouts_period_range",
                table: "payouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_payout_groups",
                table: "payout_groups");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payout_groups_date_range",
                table: "payout_groups");

            migrationBuilder.RenameTable(
                name: "payouts",
                newName: "monthly_payouts");

            migrationBuilder.RenameTable(
                name: "payout_groups",
                newName: "monthly_payout_batches");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_WalletId",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_RetryReviewedById",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_RetryReviewedById");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_RetryRequestedById",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_RetryRequestedById");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_RejectedById",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_RejectedById");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_PeriodFromDate_PeriodToDate_Status",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_PeriodFromDate_PeriodToDate_Status");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_PaidById",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_PaidById");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_MonthlyPayoutBatchId",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_MonthlyPayoutBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_payouts_ApprovedById",
                table: "monthly_payouts",
                newName: "IX_monthly_payouts_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_payout_groups_Status",
                table: "monthly_payout_batches",
                newName: "IX_monthly_payout_batches_Status");

            migrationBuilder.RenameIndex(
                name: "IX_payout_groups_FromDate_ToDate",
                table: "monthly_payout_batches",
                newName: "IX_monthly_payout_batches_FromDate_ToDate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_monthly_payouts",
                table: "monthly_payouts",
                column: "MonthlyPayoutId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_monthly_payout_batches",
                table: "monthly_payout_batches",
                column: "MonthlyPayoutBatchId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payouts_amount_positive",
                table: "monthly_payouts",
                sql: "\"Amount\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payouts_period_range",
                table: "monthly_payouts",
                sql: "\"PeriodFromDate\" <= \"PeriodToDate\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payout_batches_date_range",
                table: "monthly_payout_batches",
                sql: "\"FromDate\" <= \"ToDate\"");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_monthly_payout_batches_MonthlyPayoutBatchId",
                table: "monthly_payouts",
                column: "MonthlyPayoutBatchId",
                principalTable: "monthly_payout_batches",
                principalColumn: "MonthlyPayoutBatchId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_ApprovedById",
                table: "monthly_payouts",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_PaidById",
                table: "monthly_payouts",
                column: "PaidById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_RejectedById",
                table: "monthly_payouts",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_RetryRequestedById",
                table: "monthly_payouts",
                column: "RetryRequestedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_RetryReviewedById",
                table: "monthly_payouts",
                column: "RetryReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_wallets_WalletId",
                table: "monthly_payouts",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_monthly_payouts_MonthlyPayoutId",
                table: "wallet_transactions",
                column: "MonthlyPayoutId",
                principalTable: "monthly_payouts",
                principalColumn: "MonthlyPayoutId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
