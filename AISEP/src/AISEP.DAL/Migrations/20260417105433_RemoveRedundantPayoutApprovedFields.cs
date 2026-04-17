using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantPayoutApprovedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_ApprovedById",
                table: "payouts");

            migrationBuilder.DropIndex(
                name: "IX_payouts_ApprovedById",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "payouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "payouts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payouts_ApprovedById",
                table: "payouts",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_ApprovedById",
                table: "payouts",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
