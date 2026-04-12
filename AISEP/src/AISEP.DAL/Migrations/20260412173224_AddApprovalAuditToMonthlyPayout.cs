using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalAuditToMonthlyPayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "monthly_payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "monthly_payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_ApprovedById",
                table: "monthly_payouts",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_users_ApprovedById",
                table: "monthly_payouts",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_ApprovedById",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_ApprovedById",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "monthly_payouts");
        }
    }
}
