using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260425103000_RemoveSystemPatternKeyFromFormValidationRules")]
    public partial class RemoveSystemPatternKeyFromFormValidationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'form_validation_rules'
                          AND column_name = 'SystemPatternKey'
                    ) THEN
                        ALTER TABLE form_validation_rules
                        DROP CONSTRAINT IF EXISTS "CK_form_validation_rules_pattern_source";

                        UPDATE form_validation_rules
                        SET "CustomRegexPattern" = CASE
                            WHEN "SystemPatternKey" = 'safeText' THEN '^[\p{L}\p{N}\s.,;:!?&()%''"-]*$'
                            WHEN "SystemPatternKey" = 'email' THEN '^[^\s@]+@[^\s@]+\.[^\s@]+$'
                            WHEN "SystemPatternKey" = 'phoneVN' THEN '^(03|05|07|08|09)\d{8}$'
                            WHEN "SystemPatternKey" = 'url' THEN '^https?://.+$'
                            WHEN "SystemPatternKey" = 'ethWallet' THEN '^0x[a-fA-F0-9]{40}$'
                            ELSE "CustomRegexPattern"
                        END
                        WHERE "SystemPatternKey" IS NOT NULL;

                        ALTER TABLE form_validation_rules
                        DROP COLUMN IF EXISTS "SystemPatternKey";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemPatternKey",
                table: "form_validation_rules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_form_validation_rules_pattern_source",
                table: "form_validation_rules",
                sql: "NOT (\"SystemPatternKey\" IS NOT NULL AND \"CustomRegexPattern\" IS NOT NULL)");

            migrationBuilder.Sql("""
                UPDATE form_validation_rules
                SET "SystemPatternKey" = CASE
                    WHEN "CustomRegexPattern" = '^[\p{L}\p{N}\s.,;:!?&()%''"-]*$' THEN 'safeText'
                    WHEN "CustomRegexPattern" = '^[^\s@]+@[^\s@]+\.[^\s@]+$' THEN 'email'
                    WHEN "CustomRegexPattern" = '^(03|05|07|08|09)\d{8}$' THEN 'phoneVN'
                    WHEN "CustomRegexPattern" = '^https?://.+$' THEN 'url'
                    WHEN "CustomRegexPattern" = '^0x[a-fA-F0-9]{40}$' THEN 'ethWallet'
                    ELSE NULL
                END;
                """);
        }
    }
}
