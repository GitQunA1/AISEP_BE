using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class FixProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_UserId",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "projects",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_projects_UserId",
                table: "projects",
                newName: "IX_projects_StartupId");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_startups_StartupId",
                table: "projects",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_startups_StartupId",
                table: "projects");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "projects",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_projects_StartupId",
                table: "projects",
                newName: "IX_projects_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_UserId",
                table: "projects",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
