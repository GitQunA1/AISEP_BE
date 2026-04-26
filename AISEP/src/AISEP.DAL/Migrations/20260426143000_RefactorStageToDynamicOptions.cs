using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426143000_RefactorStageToDynamicOptions")]
    public partial class RefactorStageToDynamicOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stage_options",
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
                    table.PrimaryKey("PK_stage_options", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stage_options_Value",
                table: "stage_options",
                column: "Value",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredStageOptionId",
                table: "investors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageOptionId",
                table: "projects",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO stage_options ("Value", "IsActive", "CreatedAt", "UpdatedAt")
                VALUES
                    ('Idea', TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('MVP', TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('Growth', TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                ON CONFLICT ("Value") DO NOTHING;
                """);

            migrationBuilder.Sql("""
                UPDATE projects p
                SET "StageOptionId" = s."Id"
                FROM stage_options s
                WHERE p."DevelopmentStage" IS NOT NULL
                  AND LOWER(s."Value") = LOWER(p."DevelopmentStage");
                """);

            migrationBuilder.Sql("""
                UPDATE investors i
                SET "PreferredStageOptionId" = s."Id"
                FROM stage_options s
                WHERE i."PreferredStage" IS NOT NULL
                  AND LOWER(s."Value") = LOWER(i."PreferredStage");
                """);

            migrationBuilder.Sql("""
                UPDATE form_validation_rules
                SET "FieldKey" = 'stageOptionId'
                WHERE "FieldKey" = 'developmentStage';

                UPDATE form_validation_rules
                SET "FieldKey" = 'preferredStageOptionId'
                WHERE "FieldKey" = 'preferredStage';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_investors_PreferredStageOptionId",
                table: "investors",
                column: "PreferredStageOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_StageOptionId",
                table: "projects",
                column: "StageOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_investors_stage_options_PreferredStageOptionId",
                table: "investors",
                column: "PreferredStageOptionId",
                principalTable: "stage_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_projects_stage_options_StageOptionId",
                table: "projects",
                column: "StageOptionId",
                principalTable: "stage_options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "PreferredStage",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "DevelopmentStage",
                table: "projects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredStage",
                table: "investors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevelopmentStage",
                table: "projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE projects p
                SET "DevelopmentStage" = s."Value"
                FROM stage_options s
                WHERE p."StageOptionId" = s."Id";

                UPDATE investors i
                SET "PreferredStage" = s."Value"
                FROM stage_options s
                WHERE i."PreferredStageOptionId" = s."Id";

                UPDATE form_validation_rules
                SET "FieldKey" = 'developmentStage'
                WHERE "FieldKey" = 'stageOptionId';

                UPDATE form_validation_rules
                SET "FieldKey" = 'preferredStage'
                WHERE "FieldKey" = 'preferredStageOptionId';
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_investors_stage_options_PreferredStageOptionId",
                table: "investors");

            migrationBuilder.DropForeignKey(
                name: "FK_projects_stage_options_StageOptionId",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_investors_PreferredStageOptionId",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_projects_StageOptionId",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "PreferredStageOptionId",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "StageOptionId",
                table: "projects");

            migrationBuilder.DropTable(
                name: "stage_options");
        }
    }
}
