using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260430113000_MoveFormValidationRuleStageOptionsToJoinTable")]
    public partial class MoveFormValidationRuleStageOptionsToJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_validation_rule_stage_options",
                columns: table => new
                {
                    FormValidationRuleId = table.Column<int>(type: "integer", nullable: false),
                    StageOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_validation_rule_stage_options", x => new { x.FormValidationRuleId, x.StageOptionId });
                    table.ForeignKey(
                        name: "FK_form_validation_rule_stage_options_form_validation_rules_FormValidationRuleId",
                        column: x => x.FormValidationRuleId,
                        principalTable: "form_validation_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_form_validation_rule_stage_options_stage_options_StageOptionId",
                        column: x => x.StageOptionId,
                        principalTable: "stage_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO form_validation_rule_stage_options ("FormValidationRuleId", "StageOptionId")
                SELECT DISTINCT f."Id", stage_option_id."Value"::integer
                FROM form_validation_rules AS f
                CROSS JOIN LATERAL jsonb_array_elements_text(f."StageOptionIds"::jsonb) AS stage_option_id("Value")
                INNER JOIN stage_options AS s ON s."Id" = stage_option_id."Value"::integer
                WHERE f."StageOptionIds" IS NOT NULL
                  AND btrim(f."StageOptionIds") <> '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_form_validation_rule_stage_options_StageOptionId",
                table: "form_validation_rule_stage_options",
                column: "StageOptionId");

            migrationBuilder.DropColumn(
                name: "StageOptionIds",
                table: "form_validation_rules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StageOptionIds",
                table: "form_validation_rules",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE form_validation_rules AS f
                SET "StageOptionIds" = stage_option_summary."StageOptionIds"
                FROM (
                    SELECT "FormValidationRuleId",
                           jsonb_agg("StageOptionId" ORDER BY "StageOptionId")::text AS "StageOptionIds"
                    FROM form_validation_rule_stage_options
                    GROUP BY "FormValidationRuleId"
                ) AS stage_option_summary
                WHERE f."Id" = stage_option_summary."FormValidationRuleId";
                """);

            migrationBuilder.DropTable(
                name: "form_validation_rule_stage_options");
        }
    }
}
