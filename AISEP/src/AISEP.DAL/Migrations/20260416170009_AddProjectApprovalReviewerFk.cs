using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectApprovalReviewerFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_projects_ApprovedById",
                table: "projects",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_projects_RejectedById",
                table: "projects",
                column: "RejectedById");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_ApprovedById",
                table: "projects",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_RejectedById",
                table: "projects",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_ApprovedById",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_RejectedById",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_ApprovedById",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_RejectedById",
                table: "projects");
        }
    }
}
