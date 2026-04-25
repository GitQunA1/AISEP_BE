using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426100000_AddProfileImageUrlToInvestor")]
    public partial class AddProfileImageUrlToInvestor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "investors",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO form_validation_rules
                    ("Id", "FormKey", "FieldKey", "IsRequired", "MinLength", "MaxLength", "CustomRegexPattern", "MinValue", "MaxValue", "AllowedFileTypesJson", "MaxFileSizeBytes", "CreatedAt", "UpdatedAt")
                VALUES
                    (87, 'investor.create', 'profileImageFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                    (88, 'investor.update', 'profileImageFile', FALSE, NULL, NULL, NULL, NULL, NULL, '["image/jpeg","image/png","image/webp"]', 5242880, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                ON CONFLICT ("FormKey", "FieldKey") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM form_validation_rules
                WHERE "Id" IN (87, 88)
                   OR ("FormKey" = 'investor.create' AND "FieldKey" = 'profileImageFile')
                   OR ("FormKey" = 'investor.update' AND "FieldKey" = 'profileImageFile');
                """);

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "investors");
        }
    }
}
