using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DropAdvisorInvestorCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_advisors_users_CreatedBy",
                table: "advisors");

            migrationBuilder.DropForeignKey(
                name: "FK_investors_users_CreatedBy",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_investors_CreatedBy",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_advisors_CreatedBy",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "advisors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "investors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "advisors",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_investors_CreatedBy",
                table: "investors",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_advisors_CreatedBy",
                table: "advisors",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_advisors_users_CreatedBy",
                table: "advisors",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_investors_users_CreatedBy",
                table: "investors",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
