using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefineMonthlyPayoutBatchAndReject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "monthly_payout_batches",
                newName: "RejectedAmount");

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "monthly_payouts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "monthly_payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "monthly_payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualPayableAmount",
                table: "monthly_payout_batches",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTotalAmount",
                table: "monthly_payout_batches",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_RejectedById",
                table: "monthly_payouts",
                column: "RejectedById");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_RejectedById",
                table: "monthly_payouts",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RejectedById",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_RejectedById",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "ActualPayableAmount",
                table: "monthly_payout_batches");

            migrationBuilder.DropColumn(
                name: "EstimatedTotalAmount",
                table: "monthly_payout_batches");

            migrationBuilder.RenameColumn(
                name: "RejectedAmount",
                table: "monthly_payout_batches",
                newName: "TotalAmount");
        }
    }
}
