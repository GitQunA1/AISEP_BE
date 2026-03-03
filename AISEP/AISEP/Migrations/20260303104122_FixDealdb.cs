using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class FixDealdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deals_projects_ProjectId",
                table: "deals");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "deals",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_deals_ProjectId",
                table: "deals",
                newName: "IX_deals_StartupId");

            migrationBuilder.AddForeignKey(
                name: "FK_deals_startups_StartupId",
                table: "deals",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deals_startups_StartupId",
                table: "deals");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "deals",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_deals_StartupId",
                table: "deals",
                newName: "IX_deals_ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_deals_projects_ProjectId",
                table: "deals",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
