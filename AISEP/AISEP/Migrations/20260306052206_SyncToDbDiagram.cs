using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class SyncToDbDiagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "user_reports",
                newName: "UserReportId");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "transactions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FollowedAt",
                table: "startup_followers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "packages",
                newName: "DurationMonths");

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "startups",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FocusIndustry",
                table: "investors",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "UserReportId",
                table: "user_reports",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "transactions",
                newName: "TransactionDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "startup_followers",
                newName: "FollowedAt");

            migrationBuilder.RenameColumn(
                name: "DurationMonths",
                table: "packages",
                newName: "Duration");

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "startups",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FocusIndustry",
                table: "investors",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
