using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260504110000_RemoveScorecardWeightConfigScopeColumns")]
    public partial class RemoveScorecardWeightConfigScopeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_scorecard_weight_configs_industry_options_IndustryId",
                table: "scorecard_weight_configs");

            migrationBuilder.DropForeignKey(
                name: "FK_scorecard_weight_configs_stage_options_StageId",
                table: "scorecard_weight_configs");

            migrationBuilder.DropIndex(
                name: "IX_scorecard_weight_configs_IndustryId",
                table: "scorecard_weight_configs");

            migrationBuilder.DropIndex(
                name: "IX_scorecard_weight_configs_StageId",
                table: "scorecard_weight_configs");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "scorecard_weight_configs");

            migrationBuilder.DropColumn(
                name: "StageId",
                table: "scorecard_weight_configs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndustryId",
                table: "scorecard_weight_configs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageId",
                table: "scorecard_weight_configs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_scorecard_weight_configs_IndustryId",
                table: "scorecard_weight_configs",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_scorecard_weight_configs_StageId",
                table: "scorecard_weight_configs",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_scorecard_weight_configs_industry_options_IndustryId",
                table: "scorecard_weight_configs",
                column: "IndustryId",
                principalTable: "industry_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_scorecard_weight_configs_stage_options_StageId",
                table: "scorecard_weight_configs",
                column: "StageId",
                principalTable: "stage_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
