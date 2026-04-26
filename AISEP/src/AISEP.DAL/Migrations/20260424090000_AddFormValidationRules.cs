using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260424090000_AddFormValidationRules")]
    public partial class AddFormValidationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_validation_rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MinLength = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    CustomRegexPattern = table.Column<string>(type: "text", nullable: true),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AllowedFileTypesJson = table.Column<string>(type: "text", nullable: true),
                    MaxFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_validation_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_form_validation_rules_FormKey_FieldKey",
                table: "form_validation_rules",
                columns: new[] { "FormKey", "FieldKey" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO form_validation_rules
                    ("Id", "FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    (1, 'startup.create', 'companyName', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (2, 'startup.create', 'founder', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (3, 'startup.create', 'email', TRUE, NULL, 255, '^[^\s@]+@[^\s@]+\.[^\s@]+$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (4, 'startup.create', 'phoneNumber', TRUE, NULL, 50, '^(03|05|07|08|09)\d{8}$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (5, 'startup.create', 'countryCity', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (6, 'startup.create', 'website', FALSE, NULL, 255, '^https?://.+$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (7, 'startup.create', 'industryOptionIds', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (8, 'startup.create', 'logoFile', TRUE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (9, 'startup.create', 'businessLicenseFile', TRUE, NULL, NULL, NULL, NULL, NULL, '["application/pdf","image/jpeg","image/png"]', 10485760, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (10, 'startup.update', 'companyName', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (11, 'startup.update', 'founder', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (12, 'startup.update', 'email', FALSE, NULL, 255, '^[^\s@]+@[^\s@]+\.[^\s@]+$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (13, 'startup.update', 'phoneNumber', FALSE, NULL, 50, '^(03|05|07|08|09)\d{8}$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (14, 'startup.update', 'countryCity', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (15, 'startup.update', 'website', FALSE, NULL, 255, '^https?://.+$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (16, 'startup.update', 'industryOptionIds', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (17, 'startup.update', 'logoFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (18, 'startup.update', 'businessLicenseFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["application/pdf","image/jpeg","image/png"]', 10485760, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (19, 'investor.create', 'organizationName', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (20, 'investor.create', 'investmentTaste', TRUE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (21, 'investor.create', 'walletAddress', FALSE, NULL, 255, '^0x[a-fA-F0-9]{40}$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (22, 'investor.create', 'investmentAmount', TRUE, NULL, NULL, NULL, 0.01, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (23, 'investor.create', 'investmentRegion', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (24, 'investor.create', 'previousInvestments', TRUE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (25, 'investor.create', 'riskTolerance', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (26, 'investor.create', 'industryOptionIds', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (27, 'investor.create', 'preferredStage', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (28, 'investor.update', 'organizationName', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (29, 'investor.update', 'investmentTaste', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (30, 'investor.update', 'walletAddress', FALSE, NULL, 255, '^0x[a-fA-F0-9]{40}$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (31, 'investor.update', 'investmentAmount', FALSE, NULL, NULL, NULL, 0.01, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (32, 'investor.update', 'investmentRegion', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (33, 'investor.update', 'previousInvestments', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (34, 'investor.update', 'riskTolerance', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (35, 'investor.update', 'industryOptionIds', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (36, 'investor.update', 'preferredStage', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (37, 'advisor.create', 'bio', TRUE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (38, 'advisor.create', 'expertise', TRUE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (39, 'advisor.create', 'industryOptionIds', TRUE, 1, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (40, 'advisor.create', 'previousExperience', TRUE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (41, 'advisor.create', 'languagesSpoken', TRUE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (42, 'advisor.create', 'location', TRUE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (43, 'advisor.create', 'hourlyRate', TRUE, NULL, NULL, NULL, 0.01, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (44, 'advisor.create', 'profileImageFile', TRUE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (45, 'advisor.create', 'certificationFile', TRUE, NULL, NULL, NULL, NULL, NULL, '["application/pdf","image/jpeg","image/png"]', 10485760, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (46, 'advisor.update', 'bio', FALSE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (47, 'advisor.update', 'expertise', FALSE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (48, 'advisor.update', 'industryOptionIds', FALSE, 1, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (49, 'advisor.update', 'previousExperience', FALSE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (50, 'advisor.update', 'languagesSpoken', FALSE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (51, 'advisor.update', 'location', FALSE, NULL, NULL, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (52, 'advisor.update', 'hourlyRate', FALSE, NULL, NULL, NULL, 0.01, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (53, 'advisor.update', 'profileImageFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (54, 'advisor.update', 'certificationFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["application/pdf","image/jpeg","image/png"]', 10485760, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (55, 'project.create', 'projectName', TRUE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (56, 'project.create', 'projectImageFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (57, 'project.create', 'shortDescription', TRUE, NULL, 500, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (58, 'project.create', 'developmentStage', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (59, 'project.create', 'problemStatement', TRUE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (60, 'project.create', 'solutionDescription', TRUE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (61, 'project.create', 'targetCustomers', TRUE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (62, 'project.create', 'uniqueValueProposition', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (63, 'project.create', 'marketSize', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (64, 'project.create', 'businessModel', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (65, 'project.create', 'revenue', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (66, 'project.create', 'competitors', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (67, 'project.create', 'teamMembers', TRUE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (68, 'project.create', 'keySkills', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (69, 'project.create', 'teamExperience', FALSE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (70, 'project.create', 'industryOptionIds', TRUE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (71, 'project.update', 'projectName', FALSE, NULL, 255, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (72, 'project.update', 'projectImageFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (73, 'project.update', 'shortDescription', FALSE, NULL, 500, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (74, 'project.update', 'developmentStage', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (75, 'project.update', 'problemStatement', FALSE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (76, 'project.update', 'solutionDescription', FALSE, NULL, 2000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (77, 'project.update', 'targetCustomers', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (78, 'project.update', 'uniqueValueProposition', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (79, 'project.update', 'marketSize', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (80, 'project.update', 'businessModel', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (81, 'project.update', 'revenue', FALSE, NULL, NULL, NULL, 0.00, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (82, 'project.update', 'competitors', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (83, 'project.update', 'teamMembers', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (84, 'project.update', 'keySkills', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (85, 'project.update', 'teamExperience', FALSE, NULL, 1000, '^[\p{L}\p{N}\s.,;:!?&()%''""-]*$', NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (86, 'project.update', 'industryOptionIds', FALSE, NULL, NULL, NULL, NULL, NULL, NULL, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM form_validation_rules WHERE \"Id\" BETWEEN 1 AND 86;");

            migrationBuilder.DropTable(
                name: "form_validation_rules");
        }
    }
}
