using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    public partial class AddProjectIdToBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ProjectId",
                table: "bookings",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_projects_ProjectId",
                table: "bookings",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_projects_ProjectId",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "IX_bookings_ProjectId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "bookings");
        }
    }
}
