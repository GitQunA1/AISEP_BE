using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingFreeFlowAndUsageLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreeRebookFromComplaint",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SourceBookingId",
                table: "bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsedPremiumFreeQuota",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "premium_free_booking_usage_logs",
                columns: table => new
                {
                    PremiumFreeBookingUsageLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BookingDurationHours = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premium_free_booking_usage_logs", x => x.PremiumFreeBookingUsageLogId);
                    table.CheckConstraint("CK_premium_free_booking_usage_logs_duration_positive", "\"BookingDurationHours\" > 0");
                    table.ForeignKey(
                        name: "FK_premium_free_booking_usage_logs_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_premium_free_booking_usage_logs_subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_premium_free_booking_usage_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_SourceBookingId",
                table: "bookings",
                column: "SourceBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_premium_free_booking_usage_logs_BookingId",
                table: "premium_free_booking_usage_logs",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_premium_free_booking_usage_logs_SubscriptionId",
                table: "premium_free_booking_usage_logs",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_premium_free_booking_usage_logs_UserId_UsedAt",
                table: "premium_free_booking_usage_logs",
                columns: new[] { "UserId", "UsedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_bookings_SourceBookingId",
                table: "bookings",
                column: "SourceBookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_bookings_SourceBookingId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "premium_free_booking_usage_logs");

            migrationBuilder.DropIndex(
                name: "IX_bookings_SourceBookingId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "IsFreeRebookFromComplaint",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "SourceBookingId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "UsedPremiumFreeQuota",
                table: "bookings");
        }
    }
}
