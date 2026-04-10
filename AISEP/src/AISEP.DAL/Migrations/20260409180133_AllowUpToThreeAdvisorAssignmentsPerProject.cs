using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AllowUpToThreeAdvisorAssignmentsPerProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_project_advisor_assignments",
                table: "project_advisor_assignments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_project_advisor_assignments",
                table: "project_advisor_assignments",
                columns: new[] { "ProjectId", "AdvisorId" });

            migrationBuilder.CreateIndex(
                name: "IX_project_advisor_assignments_ProjectId",
                table: "project_advisor_assignments",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_project_advisor_assignments",
                table: "project_advisor_assignments");

            migrationBuilder.DropIndex(
                name: "IX_project_advisor_assignments_ProjectId",
                table: "project_advisor_assignments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_project_advisor_assignments",
                table: "project_advisor_assignments",
                column: "ProjectId");
        }
    }
}
