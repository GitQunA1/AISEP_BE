using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyPayoutFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyPayoutId",
                table: "wallet_transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "monthly_payouts",
                columns: table => new
                {
                    MonthlyPayoutId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidById = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_payouts", x => x.MonthlyPayoutId);
                    table.CheckConstraint("CK_monthly_payouts_amount_positive", "\"Amount\" > 0");
                    table.CheckConstraint("CK_monthly_payouts_month_range", "\"Month\" >= 1 AND \"Month\" <= 12");
                    table.ForeignKey(
                        name: "FK_monthly_payouts_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_payouts_users_PaidById",
                        column: x => x.PaidById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_payouts_wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_MonthlyPayoutId",
                table: "wallet_transactions",
                column: "MonthlyPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_AdvisorId_Year_Month",
                table: "monthly_payouts",
                columns: new[] { "AdvisorId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_PaidById",
                table: "monthly_payouts",
                column: "PaidById");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_WalletId",
                table: "monthly_payouts",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_Year_Month_Status",
                table: "monthly_payouts",
                columns: new[] { "Year", "Month", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_monthly_payouts_MonthlyPayoutId",
                table: "wallet_transactions",
                column: "MonthlyPayoutId",
                principalTable: "monthly_payouts",
                principalColumn: "MonthlyPayoutId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_monthly_payouts_MonthlyPayoutId",
                table: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_wallet_transactions_MonthlyPayoutId",
                table: "wallet_transactions");

            migrationBuilder.DropColumn(
                name: "MonthlyPayoutId",
                table: "wallet_transactions");
        }
    }
}
