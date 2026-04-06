using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWithdrawApproveRejectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.RenameColumn(
                name: "ReviewedById",
                table: "withdraw_requests",
                newName: "RejectedById");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "withdraw_requests",
                newName: "RejectedAt");

            migrationBuilder.RenameColumn(
                name: "ReviewReason",
                table: "withdraw_requests",
                newName: "RejectionReason");

            migrationBuilder.RenameIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests",
                newName: "IX_withdraw_requests_RejectedById");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ApprovedById",
                table: "withdraw_requests",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ApprovedById",
                table: "withdraw_requests",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_RejectedById",
                table: "withdraw_requests",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_RejectedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.RenameColumn(
                name: "RejectionReason",
                table: "withdraw_requests",
                newName: "ReviewReason");

            migrationBuilder.RenameColumn(
                name: "RejectedById",
                table: "withdraw_requests",
                newName: "ReviewedById");

            migrationBuilder.RenameColumn(
                name: "RejectedAt",
                table: "withdraw_requests",
                newName: "ReviewedAt");

            migrationBuilder.RenameIndex(
                name: "IX_withdraw_requests_RejectedById",
                table: "withdraw_requests",
                newName: "IX_withdraw_requests_ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
