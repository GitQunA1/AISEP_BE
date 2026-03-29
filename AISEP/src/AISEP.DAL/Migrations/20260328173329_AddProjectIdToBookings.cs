using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "project_advisor_assignments",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_advisor_assignments", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_project_advisor_assignments_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_advisor_assignments_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ProjectId",
                table: "bookings",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_advisor_assignments_AdvisorId",
                table: "project_advisor_assignments",
                column: "AdvisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_projects_ProjectId",
                table: "bookings",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_projects_ProjectId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "project_advisor_assignments");

            migrationBuilder.DropIndex(
                name: "IX_bookings_ProjectId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "bookings");
        }
    }
}
