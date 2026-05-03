using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class refactorProjectScorecard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeySkills",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "MarketSize",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Revenue",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TeamExperience",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TeamMembers",
                table: "projects");

            migrationBuilder.AlterColumn<string>(
                name: "ShortDescription",
                table: "projects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndustryOptionId",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE projects p
                SET "IndustryOptionId" = (
                    SELECT MIN(pi."IndustryOptionId")
                    FROM project_industries pi
                    WHERE pi."ProjectId" = p."ProjectId"
                );
                """);

            migrationBuilder.Sql("""
                UPDATE projects
                SET "IndustryOptionId" = (
                    SELECT MIN("Id") FROM industry_options
                )
                WHERE "IndustryOptionId" IS NULL;
                """);

            migrationBuilder.DropTable(
                name: "project_industries");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryOptionId",
                table: "projects",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "project_scorecards",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    TeamSize = table.Column<int>(type: "integer", nullable: false),
                    TeamExperience = table.Column<int>(type: "integer", nullable: false),
                    HasTechnicalCofounder = table.Column<bool>(type: "boolean", nullable: false),
                    TargetMarketSize = table.Column<int>(type: "integer", nullable: false),
                    MarketGrowth = table.Column<int>(type: "integer", nullable: false),
                    ProductReadiness = table.Column<int>(type: "integer", nullable: false),
                    IPProtection = table.Column<int>(type: "integer", nullable: false),
                    BarrierToEntry = table.Column<int>(type: "integer", nullable: false),
                    CurrentTraction = table.Column<int>(type: "integer", nullable: false),
                    RunwayMonths = table.Column<int>(type: "integer", nullable: false),
                    CalculatedScore = table.Column<decimal>(type: "numeric(6,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_scorecards", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_project_scorecards_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scorecard_weight_configs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IndustryId = table.Column<int>(type: "integer", nullable: true),
                    StageId = table.Column<int>(type: "integer", nullable: true),
                    ConfigName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TeamWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    MarketWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ProductWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CompetitionWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TractionWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    InvestmentNeedWeight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scorecard_weight_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scorecard_weight_configs_industry_options_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "industry_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scorecard_weight_configs_stage_options_StageId",
                        column: x => x.StageId,
                        principalTable: "stage_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_projects_IndustryOptionId",
                table: "projects",
                column: "IndustryOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_scorecard_weight_configs_IndustryId",
                table: "scorecard_weight_configs",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_scorecard_weight_configs_StageId",
                table: "scorecard_weight_configs",
                column: "StageId");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_industry_options_IndustryOptionId",
                table: "projects",
                column: "IndustryOptionId",
                principalTable: "industry_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_industry_options_IndustryOptionId",
                table: "projects");

            migrationBuilder.DropTable(
                name: "project_scorecards");

            migrationBuilder.DropTable(
                name: "scorecard_weight_configs");

            migrationBuilder.DropIndex(
                name: "IX_projects_IndustryOptionId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "IndustryOptionId",
                table: "projects");

            migrationBuilder.AlterColumn<string>(
                name: "ShortDescription",
                table: "projects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeySkills",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarketSize",
                table: "projects",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Revenue",
                table: "projects",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamExperience",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamMembers",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "project_industries",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    IndustryOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_industries", x => new { x.ProjectId, x.IndustryOptionId });
                    table.ForeignKey(
                        name: "FK_project_industries_industry_options_IndustryOptionId",
                        column: x => x.IndustryOptionId,
                        principalTable: "industry_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_industries_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_industries_IndustryOptionId",
                table: "project_industries",
                column: "IndustryOptionId");
        }
    }
}
