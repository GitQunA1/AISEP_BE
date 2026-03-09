using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class WalletBelongsToAdvisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallets_users_UserId",
                table: "wallets");

            migrationBuilder.DropIndex(
                name: "IX_wallets_UserId",
                table: "wallets");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "wallets",
                newName: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_wallets_AdvisorId",
                table: "wallets",
                column: "AdvisorId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_wallets_advisors_AdvisorId",
                table: "wallets",
                column: "AdvisorId",
                principalTable: "advisors",
                principalColumn: "AdvisorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_wallets_advisors_AdvisorId",
                table: "wallets");

            migrationBuilder.DropIndex(
                name: "IX_wallets_AdvisorId",
                table: "wallets");

            migrationBuilder.RenameColumn(
                name: "AdvisorId",
                table: "wallets",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_wallets_UserId",
                table: "wallets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_wallets_users_UserId",
                table: "wallets",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
