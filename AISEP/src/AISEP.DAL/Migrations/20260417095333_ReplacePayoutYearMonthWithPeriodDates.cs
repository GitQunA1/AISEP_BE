using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePayoutYearMonthWithPeriodDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_Year_Month_Status",
                table: "monthly_payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payouts_month_range",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payout_batches_Year_Month_Status",
                table: "monthly_payout_batches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payout_batches_month_range",
                table: "monthly_payout_batches");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodFromDate",
                table: "monthly_payouts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodToDate",
                table: "monthly_payouts",
                type: "date",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "monthly_payouts" AS mp
                SET "PeriodFromDate" = COALESCE(mpb."FromDate", DATE_TRUNC('day', mp."CreatedAt")::date),
                    "PeriodToDate" = COALESCE(mpb."ToDate", DATE_TRUNC('day', mp."CreatedAt")::date)
                FROM "monthly_payout_batches" AS mpb
                WHERE mp."MonthlyPayoutBatchId" = mpb."MonthlyPayoutBatchId";
                """);

            migrationBuilder.Sql(
                """
                UPDATE "monthly_payouts"
                SET "PeriodFromDate" = DATE_TRUNC('day', "CreatedAt")::date,
                    "PeriodToDate" = DATE_TRUNC('day', "CreatedAt")::date
                WHERE "PeriodFromDate" IS NULL OR "PeriodToDate" IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodFromDate",
                table: "monthly_payouts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PeriodToDate",
                table: "monthly_payouts",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Month",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "monthly_payout_batches");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "monthly_payout_batches");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_PeriodFromDate_PeriodToDate_Status",
                table: "monthly_payouts",
                columns: new[] { "PeriodFromDate", "PeriodToDate", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payouts_period_range",
                table: "monthly_payouts",
                sql: "\"PeriodFromDate\" <= \"PeriodToDate\"");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_Status",
                table: "monthly_payout_batches",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_PeriodFromDate_PeriodToDate_Status",
                table: "monthly_payouts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_monthly_payouts_period_range",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payout_batches_Status",
                table: "monthly_payout_batches");

            migrationBuilder.DropColumn(
                name: "PeriodFromDate",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "PeriodToDate",
                table: "monthly_payouts");

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "monthly_payouts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "monthly_payouts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "monthly_payout_batches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "monthly_payout_batches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_Year_Month_Status",
                table: "monthly_payouts",
                columns: new[] { "Year", "Month", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payouts_month_range",
                table: "monthly_payouts",
                sql: "\"Month\" >= 1 AND \"Month\" <= 12");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_Year_Month_Status",
                table: "monthly_payout_batches",
                columns: new[] { "Year", "Month", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_monthly_payout_batches_month_range",
                table: "monthly_payout_batches",
                sql: "\"Month\" >= 1 AND \"Month\" <= 12");
        }
    }
}
