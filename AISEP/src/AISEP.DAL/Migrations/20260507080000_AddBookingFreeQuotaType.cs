using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260507080000_AddBookingFreeQuotaType")]
    public partial class AddBookingFreeQuotaType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FreeQuotaType",
                table: "bookings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.Sql("""
                UPDATE bookings
                SET "FreeQuotaType" = CASE
                    WHEN "UsedPremiumFreeQuota" = TRUE THEN 'Premium'
                    WHEN "IsPaymentWaived" = TRUE THEN 'Bonus'
                    ELSE 'None'
                END
                """);

            migrationBuilder.DropColumn(
                name: "IsPaymentWaived",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "UsedPremiumFreeQuota",
                table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentWaived",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsedPremiumFreeQuota",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE bookings
                SET
                    "IsPaymentWaived" = CASE WHEN "FreeQuotaType" <> 'None' THEN TRUE ELSE FALSE END,
                    "UsedPremiumFreeQuota" = CASE WHEN "FreeQuotaType" = 'Premium' THEN TRUE ELSE FALSE END
                """);

            migrationBuilder.DropColumn(
                name: "FreeQuotaType",
                table: "bookings");
        }
    }
}
