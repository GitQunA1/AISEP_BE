using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class updatedbfixAIAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_documents_projects_ProjectId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_investor_ai_analyses_projects_ProjectId",
                table: "investor_ai_analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_startup_ai_analyses_projects_ProjectId",
                table: "startup_ai_analyses");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "startup_ai_analyses",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_startup_ai_analyses_ProjectId",
                table: "startup_ai_analyses",
                newName: "IX_startup_ai_analyses_StartupId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "investor_ai_analyses",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_investor_ai_analyses_ProjectId",
                table: "investor_ai_analyses",
                newName: "IX_investor_ai_analyses_StartupId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "documents",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_ProjectId",
                table: "documents",
                newName: "IX_documents_StartupId");

            migrationBuilder.AddForeignKey(
                name: "FK_documents_startups_StartupId",
                table: "documents",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_investor_ai_analyses_startups_StartupId",
                table: "investor_ai_analyses",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_startup_ai_analyses_startups_StartupId",
                table: "startup_ai_analyses",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_documents_startups_StartupId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_investor_ai_analyses_startups_StartupId",
                table: "investor_ai_analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_startup_ai_analyses_startups_StartupId",
                table: "startup_ai_analyses");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "startup_ai_analyses",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_startup_ai_analyses_StartupId",
                table: "startup_ai_analyses",
                newName: "IX_startup_ai_analyses_ProjectId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "investor_ai_analyses",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_investor_ai_analyses_StartupId",
                table: "investor_ai_analyses",
                newName: "IX_investor_ai_analyses_ProjectId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "documents",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_StartupId",
                table: "documents",
                newName: "IX_documents_ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_documents_projects_ProjectId",
                table: "documents",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_investor_ai_analyses_projects_ProjectId",
                table: "investor_ai_analyses",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_startup_ai_analyses_projects_ProjectId",
                table: "startup_ai_analyses",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
