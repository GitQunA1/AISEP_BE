using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingBoundUserReportsResolutionMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_reports_ReporterId",
                table: "user_reports");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "user_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNote",
                table: "user_reports",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "user_reports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedById",
                table: "user_reports",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE user_reports
                SET ""Category"" = CASE ""Category""
                    WHEN 'UnprofessionalBehavior' THEN 'UnprofessionalConduct'
                    WHEN 'PaymentDispute' THEN 'PaymentIssue'
                    WHEN 'Harassment' THEN 'UnprofessionalConduct'
                    WHEN 'Scam' THEN 'Other'
                    WHEN 'Impersonation' THEN 'Other'
                    ELSE ""Category""
                END;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_BookingId",
                table: "user_reports",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReporterId_BookingId_Status",
                table: "user_reports",
                columns: new[] { "ReporterId", "BookingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ResolvedById",
                table: "user_reports",
                column: "ResolvedById");

            migrationBuilder.AddForeignKey(
                name: "FK_user_reports_bookings_BookingId",
                table: "user_reports",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_reports_users_ResolvedById",
                table: "user_reports",
                column: "ResolvedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_reports_bookings_BookingId",
                table: "user_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_user_reports_users_ResolvedById",
                table: "user_reports");

            migrationBuilder.DropIndex(
                name: "IX_user_reports_BookingId",
                table: "user_reports");

            migrationBuilder.DropIndex(
                name: "IX_user_reports_ReporterId_BookingId_Status",
                table: "user_reports");

            migrationBuilder.DropIndex(
                name: "IX_user_reports_ResolvedById",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "ResolutionNote",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "user_reports");

            migrationBuilder.DropColumn(
                name: "ResolvedById",
                table: "user_reports");

            migrationBuilder.Sql(@"
                UPDATE user_reports
                SET ""Category"" = CASE ""Category""
                    WHEN 'UnprofessionalConduct' THEN 'UnprofessionalBehavior'
                    WHEN 'PaymentIssue' THEN 'PaymentDispute'
                    ELSE ""Category""
                END;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_user_reports_ReporterId",
                table: "user_reports",
                column: "ReporterId");
        }
    }
}
