using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWithdrawFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_wallet_transactions_WithdrawRequestId",
                table: "wallet_transactions");

            migrationBuilder.DropColumn(
                name: "WithdrawRequestId",
                table: "wallet_transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WithdrawRequestId",
                table: "wallet_transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "withdraw_requests",
                columns: table => new
                {
                    WithdrawRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedById = table.Column<int>(type: "integer", nullable: true),
                    RejectedById = table.Column<int>(type: "integer", nullable: true),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BankAccount = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProofImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_withdraw_requests", x => x.WithdrawRequestId);
                    table.CheckConstraint("CK_withdraw_requests_amount_positive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_withdraw_requests_users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_withdraw_requests_users_RejectedById",
                        column: x => x.RejectedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_withdraw_requests_wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_WithdrawRequestId",
                table: "wallet_transactions",
                column: "WithdrawRequestId",
                unique: true,
                filter: "\"WithdrawRequestId\" IS NOT NULL AND \"Type\" = 'Withdrawal'");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ApprovedById",
                table: "withdraw_requests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_RejectedById",
                table: "withdraw_requests",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_WalletId",
                table: "withdraw_requests",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_withdraw_requests_WithdrawRequestId",
                table: "wallet_transactions",
                column: "WithdrawRequestId",
                principalTable: "withdraw_requests",
                principalColumn: "WithdrawRequestId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
