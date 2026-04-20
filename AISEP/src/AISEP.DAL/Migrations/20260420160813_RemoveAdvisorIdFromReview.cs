using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdvisorIdFromReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reviews_advisors_AdvisorId",
                table: "reviews");

            migrationBuilder.DropIndex(
                name: "IX_reviews_AdvisorId",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "AdvisorId",
                table: "reviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvisorId",
                table: "reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_reviews_AdvisorId",
                table: "reviews",
                column: "AdvisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_advisors_AdvisorId",
                table: "reviews",
                column: "AdvisorId",
                principalTable: "advisors",
                principalColumn: "AdvisorId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
