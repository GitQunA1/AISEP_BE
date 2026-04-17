using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovePayoutRetryRequestedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payouts_users_RetryRequestedById",
                table: "payouts");

            migrationBuilder.DropIndex(
                name: "IX_payouts_RetryRequestedById",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "RetryRequestedById",
                table: "payouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RetryRequestedById",
                table: "payouts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_payouts_RetryRequestedById",
                table: "payouts",
                column: "RetryRequestedById");

            migrationBuilder.AddForeignKey(
                name: "FK_payouts_users_RetryRequestedById",
                table: "payouts",
                column: "RetryRequestedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
