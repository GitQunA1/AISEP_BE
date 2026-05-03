using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260503143000_UpdateProjectScorecardValidationRules")]
    public partial class UpdateProjectScorecardValidationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM form_validation_rules
                WHERE "FormKey" IN ('project.create', 'project.update')
                  AND "FieldKey" IN ('marketSize', 'revenue', 'teamMembers', 'keySkills', 'teamExperience');

                INSERT INTO form_validation_rules
                    ("FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    ('project.create', 'teamSize', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'teamExperience', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'hasTechnicalCofounder', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'targetMarketSize', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'marketGrowth', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'productReadiness', TRUE, NULL, NULL, NULL, 1.00, 4.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'ipProtection', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'barrierToEntry', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'currentTraction', TRUE, NULL, NULL, NULL, 1.00, 4.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'runwayMonths', TRUE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'teamSize', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'teamExperience', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'hasTechnicalCofounder', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'targetMarketSize', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'marketGrowth', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'productReadiness', FALSE, NULL, NULL, NULL, 1.00, 4.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'ipProtection', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'barrierToEntry', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'currentTraction', FALSE, NULL, NULL, NULL, 1.00, 4.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'runwayMonths', FALSE, NULL, NULL, NULL, 1.00, 3.00, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
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
                  AND "FieldKey" IN (
                      'teamSize',
                      'teamExperience',
                      'hasTechnicalCofounder',
                      'targetMarketSize',
                      'marketGrowth',
                      'productReadiness',
                      'ipProtection',
                      'barrierToEntry',
                      'currentTraction',
                      'runwayMonths'
                  );

                INSERT INTO form_validation_rules
                    ("FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    ('project.create', 'marketSize', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'revenue', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'teamMembers', TRUE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'keySkills', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.create', 'teamExperience', FALSE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'marketSize', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'revenue', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'teamMembers', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'keySkills', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    ('project.update', 'teamExperience', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
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
