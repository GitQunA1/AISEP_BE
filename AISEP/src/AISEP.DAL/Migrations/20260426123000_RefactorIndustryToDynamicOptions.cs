using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426123000_RefactorIndustryToDynamicOptions")]
    public partial class RefactorIndustryToDynamicOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "industry_options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industry_options", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_industry_options_Value",
                table: "industry_options",
                column: "Value",
                unique: true);

            migrationBuilder.CreateTable(
                name: "startup_industries",
                columns: table => new
                {
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    IndustryOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_startup_industries", x => new { x.StartupId, x.IndustryOptionId });
                    table.ForeignKey(
                        name: "FK_startup_industries_industry_options_IndustryOptionId",
                        column: x => x.IndustryOptionId,
                        principalTable: "industry_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_startup_industries_startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "startups",
                        principalColumn: "StartupId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "investor_industries",
                columns: table => new
                {
                    InvestorId = table.Column<int>(type: "integer", nullable: false),
                    IndustryOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_investor_industries", x => new { x.InvestorId, x.IndustryOptionId });
                    table.ForeignKey(
                        name: "FK_investor_industries_industry_options_IndustryOptionId",
                        column: x => x.IndustryOptionId,
                        principalTable: "industry_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_investor_industries_investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "investors",
                        principalColumn: "InvestorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_startup_industries_IndustryOptionId",
                table: "startup_industries",
                column: "IndustryOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_project_industries_IndustryOptionId",
                table: "project_industries",
                column: "IndustryOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_investor_industries_IndustryOptionId",
                table: "investor_industries",
                column: "IndustryOptionId");

            migrationBuilder.AddColumn<int>(
                name: "IndustryOptionId",
                table: "advisor_industries",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO industry_options ("Value", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT DISTINCT src.industry_value, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                FROM (
                    SELECT "Industry" AS industry_value FROM startups WHERE "Industry" IS NOT NULL
                    UNION
                    SELECT "FocusIndustry" AS industry_value FROM investors WHERE "FocusIndustry" IS NOT NULL
                    UNION
                    SELECT "Industry" AS industry_value FROM projects WHERE "Industry" IS NOT NULL
                    UNION
                    SELECT "Industry" AS industry_value FROM advisor_industries WHERE "Industry" IS NOT NULL
                ) src
                WHERE src.industry_value IS NOT NULL
                ON CONFLICT ("Value") DO NOTHING;
                """);

            migrationBuilder.Sql("""
                INSERT INTO startup_industries ("StartupId", "IndustryOptionId")
                SELECT s."StartupId", io."Id"
                FROM startups s
                JOIN industry_options io ON io."Value" = s."Industry"
                WHERE s."Industry" IS NOT NULL
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql("""
                INSERT INTO investor_industries ("InvestorId", "IndustryOptionId")
                SELECT i."InvestorId", io."Id"
                FROM investors i
                JOIN industry_options io ON io."Value" = i."FocusIndustry"
                WHERE i."FocusIndustry" IS NOT NULL
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql("""
                INSERT INTO project_industries ("ProjectId", "IndustryOptionId")
                SELECT p."ProjectId", io."Id"
                FROM projects p
                JOIN industry_options io ON io."Value" = p."Industry"
                WHERE p."Industry" IS NOT NULL
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql("""
                UPDATE advisor_industries ai
                SET "IndustryOptionId" = io."Id"
                FROM industry_options io
                WHERE io."Value" = ai."Industry";
                """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_advisor_industries",
                table: "advisor_industries");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "advisor_industries");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryOptionId",
                table: "advisor_industries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_advisor_industries",
                table: "advisor_industries",
                columns: new[] { "AdvisorId", "IndustryOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_advisor_industries_IndustryOptionId",
                table: "advisor_industries",
                column: "IndustryOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_advisor_industries_industry_options_IndustryOptionId",
                table: "advisor_industries",
                column: "IndustryOptionId",
                principalTable: "industry_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "FocusIndustry",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "projects");

            migrationBuilder.Sql("""
                UPDATE form_validation_rules
                SET "FieldKey" = 'industryOptionIds'
                WHERE ("FormKey" = 'startup.create' AND "FieldKey" = 'industry')
                   OR ("FormKey" = 'startup.update' AND "FieldKey" = 'industry')
                   OR ("FormKey" = 'investor.create' AND "FieldKey" = 'focusIndustry')
                   OR ("FormKey" = 'investor.update' AND "FieldKey" = 'focusIndustry')
                   OR ("FormKey" = 'advisor.create' AND "FieldKey" = 'industries')
                   OR ("FormKey" = 'advisor.update' AND "FieldKey" = 'industries')
                   OR ("FormKey" = 'project.create' AND "FieldKey" = 'industry')
                   OR ("FormKey" = 'project.update' AND "FieldKey" = 'industry');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE form_validation_rules
                SET "FieldKey" = CASE
                    WHEN "FormKey" IN ('startup.create', 'startup.update') THEN 'industry'
                    WHEN "FormKey" IN ('investor.create', 'investor.update') THEN 'focusIndustry'
                    WHEN "FormKey" IN ('advisor.create', 'advisor.update') THEN 'industries'
                    WHEN "FormKey" IN ('project.create', 'project.update') THEN 'industry'
                    ELSE "FieldKey"
                END
                WHERE "FieldKey" = 'industryOptionIds';
                """);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FocusIndustry",
                table: "investors",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE startups s
                SET "Industry" = sub."Value"
                FROM (
                    SELECT si."StartupId", MIN(io."Value") AS "Value"
                    FROM startup_industries si
                    JOIN industry_options io ON io."Id" = si."IndustryOptionId"
                    GROUP BY si."StartupId"
                ) sub
                WHERE s."StartupId" = sub."StartupId";
                """);

            migrationBuilder.Sql("""
                UPDATE investors i
                SET "FocusIndustry" = sub."Value"
                FROM (
                    SELECT ii."InvestorId", MIN(io."Value") AS "Value"
                    FROM investor_industries ii
                    JOIN industry_options io ON io."Id" = ii."IndustryOptionId"
                    GROUP BY ii."InvestorId"
                ) sub
                WHERE i."InvestorId" = sub."InvestorId";
                """);

            migrationBuilder.Sql("""
                UPDATE projects p
                SET "Industry" = sub."Value"
                FROM (
                    SELECT pi."ProjectId", MIN(io."Value") AS "Value"
                    FROM project_industries pi
                    JOIN industry_options io ON io."Id" = pi."IndustryOptionId"
                    GROUP BY pi."ProjectId"
                ) sub
                WHERE p."ProjectId" = sub."ProjectId";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_advisor_industries_industry_options_IndustryOptionId",
                table: "advisor_industries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_advisor_industries",
                table: "advisor_industries");

            migrationBuilder.DropIndex(
                name: "IX_advisor_industries_IndustryOptionId",
                table: "advisor_industries");

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "advisor_industries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE advisor_industries ai
                SET "Industry" = io."Value"
                FROM industry_options io
                WHERE io."Id" = ai."IndustryOptionId";
                """);

            migrationBuilder.DropColumn(
                name: "IndustryOptionId",
                table: "advisor_industries");

            migrationBuilder.AlterColumn<string>(
                name: "Industry",
                table: "advisor_industries",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_advisor_industries",
                table: "advisor_industries",
                columns: new[] { "AdvisorId", "Industry" });

            migrationBuilder.DropTable(name: "startup_industries");
            migrationBuilder.DropTable(name: "project_industries");
            migrationBuilder.DropTable(name: "investor_industries");
            migrationBuilder.DropTable(name: "industry_options");
        }
    }
}
