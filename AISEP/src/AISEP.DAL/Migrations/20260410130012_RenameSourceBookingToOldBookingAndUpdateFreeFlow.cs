using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameSourceBookingToOldBookingAndUpdateFreeFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_bookings_SourceBookingId",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "SourceBookingId",
                table: "bookings",
                newName: "OldBookingId");

            migrationBuilder.RenameIndex(
                name: "IX_bookings_SourceBookingId",
                table: "bookings",
                newName: "IX_bookings_OldBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_bookings_OldBookingId",
                table: "bookings",
                column: "OldBookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_bookings_OldBookingId",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "OldBookingId",
                table: "bookings",
                newName: "SourceBookingId");

            migrationBuilder.RenameIndex(
                name: "IX_bookings_OldBookingId",
                table: "bookings",
                newName: "IX_bookings_SourceBookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_bookings_SourceBookingId",
                table: "bookings",
                column: "SourceBookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
