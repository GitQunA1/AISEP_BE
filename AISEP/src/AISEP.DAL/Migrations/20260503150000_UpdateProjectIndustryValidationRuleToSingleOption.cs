using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260503150000_UpdateProjectIndustryValidationRuleToSingleOption")]
    public partial class UpdateProjectIndustryValidationRuleToSingleOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM form_validation_rules
                WHERE "FormKey" IN ('project.create', 'project.update')
                  AND "FieldKey" = 'industryOptionIds';

                SELECT setval(
                    pg_get_serial_sequence('form_validation_rules', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM form_validation_rules), 1),
                    true
                );

                INSERT INTO form_validation_rules
                    ("FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    ('project.create', 'industryOptionId', TRUE, NULL, NULL, NULL, 1.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'industryOptionId', FALSE, NULL, NULL, NULL, 1.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                ON CONFLICT ("FormKey", "FieldKey") DO UPDATE
                SET "IsRequired" = EXCLUDED."IsRequired",
                    "MinLength" = EXCLUDED."MinLength",
                    "MaxLength" = EXCLUDED."MaxLength",
                    "CustomRegexPattern" = EXCLUDED."CustomRegexPattern",
                    "MinValue" = EXCLUDED."MinValue",
                    "MaxValue" = EXCLUDED."MaxValue",
                    "AllowedFileTypesJson" = EXCLUDED."AllowedFileTypesJson",
                    "MaxFileSizeBytes" = EXCLUDED."MaxFileSizeBytes",
                    "UpdatedAt" = CURRENT_TIMESTAMP;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM form_validation_rules
                WHERE "FormKey" IN ('project.create', 'project.update')
                  AND "FieldKey" = 'industryOptionId';

                SELECT setval(
                    pg_get_serial_sequence('form_validation_rules', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM form_validation_rules), 1),
                    true
                );

                INSERT INTO form_validation_rules
                    ("FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    ('project.create', 'industryOptionIds', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'industryOptionIds', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                ON CONFLICT ("FormKey", "FieldKey") DO UPDATE
                SET "IsRequired" = EXCLUDED."IsRequired",
                    "MinLength" = EXCLUDED."MinLength",
                    "MaxLength" = EXCLUDED."MaxLength",
                    "CustomRegexPattern" = EXCLUDED."CustomRegexPattern",
                    "MinValue" = EXCLUDED."MinValue",
                    "MaxValue" = EXCLUDED."MaxValue",
                    "AllowedFileTypesJson" = EXCLUDED."AllowedFileTypesJson",
                    "MaxFileSizeBytes" = EXCLUDED."MaxFileSizeBytes",
                    "UpdatedAt" = CURRENT_TIMESTAMP;
                """);
        }
    }
}
