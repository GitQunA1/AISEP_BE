using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyPayoutBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyPayoutBatchId",
                table: "monthly_payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "monthly_payout_batches",
                columns: table => new
                {
                    MonthlyPayoutBatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_payout_batches", x => x.MonthlyPayoutBatchId);
                    table.CheckConstraint("CK_monthly_payout_batches_month_range", "\"Month\" >= 1 AND \"Month\" <= 12");
                });

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_MonthlyPayoutBatchId",
                table: "monthly_payouts",
                column: "MonthlyPayoutBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_Year_Month",
                table: "monthly_payout_batches",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payout_batches_Year_Month_Status",
                table: "monthly_payout_batches",
                columns: new[] { "Year", "Month", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_monthly_payout_batches_MonthlyPayoutBatchId",
                table: "monthly_payouts",
                column: "MonthlyPayoutBatchId",
                principalTable: "monthly_payout_batches",
                principalColumn: "MonthlyPayoutBatchId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_monthly_payout_batches_MonthlyPayoutBatchId",
                table: "monthly_payouts");

            migrationBuilder.DropTable(
                name: "monthly_payout_batches");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_MonthlyPayoutBatchId",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "MonthlyPayoutBatchId",
                table: "monthly_payouts");
        }
    }
}
