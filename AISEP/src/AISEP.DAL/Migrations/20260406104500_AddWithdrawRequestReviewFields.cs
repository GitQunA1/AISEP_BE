using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    public partial class AddWithdrawRequestReviewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "withdraw_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "withdraw_requests");
        }
    }
}
