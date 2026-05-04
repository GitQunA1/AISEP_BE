using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260504105000_AddHybridAiAnalysisScores")]
    public partial class AddHybridAiAnalysisScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PotentialScore",
                table: "project_ai_evaluations",
                newName: "FinalPotentialScore");

            migrationBuilder.AlterColumn<decimal>(
                name: "FinalPotentialScore",
                table: "project_ai_evaluations",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AIAdjustmentScore",
                table: "project_ai_evaluations",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseScore",
                table: "project_ai_evaluations",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AIAdjustmentScore",
                table: "investor_ai_analyses",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseScore",
                table: "investor_ai_analyses",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPotentialScore",
                table: "investor_ai_analyses",
                type: "numeric(6,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIAdjustmentScore",
                table: "project_ai_evaluations");

            migrationBuilder.DropColumn(
                name: "BaseScore",
                table: "project_ai_evaluations");

            migrationBuilder.DropColumn(
                name: "AIAdjustmentScore",
                table: "investor_ai_analyses");

            migrationBuilder.DropColumn(
                name: "BaseScore",
                table: "investor_ai_analyses");

            migrationBuilder.DropColumn(
                name: "FinalPotentialScore",
                table: "investor_ai_analyses");

            migrationBuilder.AlterColumn<int>(
                name: "FinalPotentialScore",
                table: "project_ai_evaluations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,2)",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "FinalPotentialScore",
                table: "project_ai_evaluations",
                newName: "PotentialScore");
        }
    }
}
