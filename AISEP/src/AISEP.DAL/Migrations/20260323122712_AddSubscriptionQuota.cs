using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemainingFreeBookings",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedAiRequests",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedProjectViews",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeBookingCount",
                table: "packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAiRequests",
                table: "packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxProjectViews",
                table: "packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingFreeBookings",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedAiRequests",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedProjectViews",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "FreeBookingCount",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "MaxAiRequests",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "MaxProjectViews",
                table: "packages");
        }
    }
}
