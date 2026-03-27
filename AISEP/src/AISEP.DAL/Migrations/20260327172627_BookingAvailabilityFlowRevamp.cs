using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class BookingAvailabilityFlowRevamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_UserId",
                table: "notifications");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "bookings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "advisor_availabilities",
                columns: table => new
                {
                    AdvisorAvailabilityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    SlotDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advisor_availabilities", x => x.AdvisorAvailabilityId);
                    table.ForeignKey(
                        name: "FK_advisor_availabilities_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_slots",
                columns: table => new
                {
                    BookingSlotId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    AdvisorAvailabilityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_slots", x => x.BookingSlotId);
                    table.ForeignKey(
                        name: "FK_booking_slots_advisor_availabilities_AdvisorAvailabilityId",
                        column: x => x.AdvisorAvailabilityId,
                        principalTable: "advisor_availabilities",
                        principalColumn: "AdvisorAvailabilityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_slots_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId_CreatedAt",
                table: "notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_advisor_availabilities_AdvisorId_SlotDate_StartTime_EndTime",
                table: "advisor_availabilities",
                columns: new[] { "AdvisorId", "SlotDate", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_slots_AdvisorAvailabilityId",
                table: "booking_slots",
                column: "AdvisorAvailabilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_slots_BookingId_AdvisorAvailabilityId",
                table: "booking_slots",
                columns: new[] { "BookingId", "AdvisorAvailabilityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_slots");

            migrationBuilder.DropTable(
                name: "advisor_availabilities");

            migrationBuilder.DropIndex(
                name: "IX_notifications_UserId_CreatedAt",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "bookings");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UserId",
                table: "notifications",
                column: "UserId");
        }
    }
}
