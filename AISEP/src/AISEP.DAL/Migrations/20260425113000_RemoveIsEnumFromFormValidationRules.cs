using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260425113000_RemoveIsEnumFromFormValidationRules")]
    public partial class RemoveIsEnumFromFormValidationRules : Migration
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
                          AND column_name = 'IsEnum'
                    ) THEN
                        ALTER TABLE form_validation_rules
                        DROP COLUMN IF EXISTS "IsEnum";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'form_validation_rules'
                          AND column_name = 'IsEnum'
                    ) THEN
                        ALTER TABLE form_validation_rules
                        ADD COLUMN "IsEnum" boolean NOT NULL DEFAULT FALSE;
                    END IF;
                END $$;
                """);
        }
    }
}
