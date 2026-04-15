using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutRetryRequestFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RetryRequestNote",
                table: "monthly_payouts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetryRequestedAt",
                table: "monthly_payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryRequestedById",
                table: "monthly_payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetryReviewNote",
                table: "monthly_payouts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetryReviewedAt",
                table: "monthly_payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryReviewedById",
                table: "monthly_payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_RetryRequestedById",
                table: "monthly_payouts",
                column: "RetryRequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_RetryReviewedById",
                table: "monthly_payouts",
                column: "RetryReviewedById");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RetryRequestedById",
                table: "monthly_payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_users_RetryReviewedById",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_RetryRequestedById",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_RetryReviewedById",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryRequestNote",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryRequestedAt",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryRequestedById",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewNote",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewedAt",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewedById",
                table: "monthly_payouts");
        }
    }
}
