using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeMonthlyPayoutAdvisorRedundancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_monthly_payouts_advisors_AdvisorId",
                table: "monthly_payouts");

            migrationBuilder.DropIndex(
                name: "IX_monthly_payouts_AdvisorId",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "AdvisorId",
                table: "monthly_payouts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvisorId",
                table: "monthly_payouts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_payouts_AdvisorId",
                table: "monthly_payouts",
                column: "AdvisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_monthly_payouts_advisors_AdvisorId",
                table: "monthly_payouts",
                column: "AdvisorId",
                principalTable: "advisors",
                principalColumn: "AdvisorId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
