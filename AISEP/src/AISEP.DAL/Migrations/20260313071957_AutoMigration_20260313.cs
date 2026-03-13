using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class AutoMigration_20260313 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "startups");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "startups",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "startups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "startups");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "startups",
                type: "text",
                nullable: true);
        }
    }
}
