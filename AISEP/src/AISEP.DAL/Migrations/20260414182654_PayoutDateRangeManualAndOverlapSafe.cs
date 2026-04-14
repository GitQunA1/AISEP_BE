using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class PayoutDateRangeManualAndOverlapSafe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_AdvisorId_Year_Month",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payout_batches_Year_Month",
                table: "monthly_payout_batches");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "monthly_payout_batches",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "monthly_payout_batches",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_AdvisorId",
                table: "monthly_payouts",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_FromDate_ToDate",
                table: "monthly_payout_batches",
                columns: new[] { "FromDate", "ToDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payout_batches_date_range",
                table: "monthly_payout_batches",
                sql: "\"FromDate\" <= \"ToDate\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_AdvisorId",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payout_batches_FromDate_ToDate",
                table: "monthly_payout_batches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payout_batches_date_range",
                table: "monthly_payout_batches");

            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "monthly_payout_batches");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "monthly_payout_batches");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_AdvisorId_Year_Month",
                table: "monthly_payouts",
                columns: new[] { "AdvisorId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_Year_Month",
                table: "monthly_payout_batches",
                columns: new[] { "Year", "Month" },
                unique: true);
        }
    }
}
