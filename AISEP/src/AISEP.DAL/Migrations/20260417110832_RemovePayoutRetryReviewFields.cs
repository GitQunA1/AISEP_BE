using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovePayoutRetryReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_RetryReviewedById",
                table: "payouts");

            migrationBuilder.DropIndex(
                name: "IX_payouts_RetryReviewedById",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewNote",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewedAt",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "RetryReviewedById",
                table: "payouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RetryReviewNote",
                table: "payouts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetryReviewedAt",
                table: "payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryReviewedById",
                table: "payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payouts_RetryReviewedById",
                table: "payouts",
                column: "RetryReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_RetryReviewedById",
                table: "payouts",
                column: "RetryReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
