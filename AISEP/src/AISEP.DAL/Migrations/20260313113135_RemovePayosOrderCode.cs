using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class RemovePayosOrderCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayosOrderCode",
                table: "transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayosOrderCode",
                table: "transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
