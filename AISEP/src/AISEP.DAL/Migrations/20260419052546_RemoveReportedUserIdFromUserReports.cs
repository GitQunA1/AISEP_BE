using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReportedUserIdFromUserReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_reports_users_ReportedUserId",
                table: "user_reports");

            migrationBuilder.DropIndex(
                name: "IX_user_reports_ReportedUserId",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "ReportedUserId",
                table: "user_reports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportedUserId",
                table: "user_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReportedUserId",
                table: "user_reports",
                column: "ReportedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_reports_users_ReportedUserId",
                table: "user_reports",
                column: "ReportedUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
