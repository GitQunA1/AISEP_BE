using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AISEP.DAL.Data;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260421090000_RemoveObsoleteBookingFlags")]
    public partial class RemoveObsoleteBookingFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreeRebookFromComplaint",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "PremiumFreeQuotaRefunded",
                table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreeRebookFromComplaint",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PremiumFreeQuotaRefunded",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
