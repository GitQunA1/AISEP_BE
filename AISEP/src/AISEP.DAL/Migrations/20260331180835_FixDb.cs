using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Industry",
                table: "advisors");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "user_reports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "user_reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EvidenceImageUrls",
                table: "user_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoEvidenceUrl",
                table: "user_reports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvisorPayoutAmount",
                table: "consulting_reports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdvisorRevisionDueAt",
                table: "consulting_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayoutProcessed",
                table: "consulting_reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSubmittedAt",
                table: "consulting_reports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PayoutProcessedAt",
                table: "consulting_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionCount",
                table: "consulting_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RevisionRequestReason",
                table: "consulting_reports",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartupReviewDueAt",
                table: "consulting_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartupReviewedAt",
                table: "consulting_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "consulting_reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "advisor_industries",
                columns: table => new
                {
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    Industry = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advisor_industries", x => new { x.AdvisorId, x.Industry });
                    table.ForeignKey(
                        name: "FK_advisor_industries_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advisor_industries");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "EvidenceImageUrls",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "VideoEvidenceUrl",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "AdvisorPayoutAmount",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "AdvisorRevisionDueAt",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "IsPayoutProcessed",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "LastSubmittedAt",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "PayoutProcessedAt",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "RevisionCount",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "RevisionRequestReason",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "StartupReviewDueAt",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "StartupReviewedAt",
                table: "consulting_reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "consulting_reports");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "user_reports",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "advisors",
                type: "text",
                nullable: true);
        }
    }
}
