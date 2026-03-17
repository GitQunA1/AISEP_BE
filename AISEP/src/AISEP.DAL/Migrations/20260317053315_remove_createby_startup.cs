using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class remove_createby_startup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_startups_users_CreatedBy",
                table: "startups");

            migrationBuilder.DropIndex(
                name: "IX_startups_CreatedBy",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "startups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "startups",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_startups_CreatedBy",
                table: "startups",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_startups_users_CreatedBy",
                table: "startups",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
