using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueAdvisorAvailabilityIndexOnBookingSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_slots_AdvisorAvailabilityId",
                table: "booking_slots");

            migrationBuilder.CreateIndex(
                name: "IX_booking_slots_AdvisorAvailabilityId",
                table: "booking_slots",
                column: "AdvisorAvailabilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_slots_AdvisorAvailabilityId",
                table: "booking_slots");

            migrationBuilder.CreateIndex(
                name: "IX_booking_slots_AdvisorAvailabilityId",
                table: "booking_slots",
                column: "AdvisorAvailabilityId",
                unique: true);
        }
    }
}
