using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemTermsAndUserTermsAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTermsAccepted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TermsAcceptedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsVersion",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "system_terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_terms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_terms_IsActive",
                table: "system_terms",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_terms");

            migrationBuilder.DropColumn(
                name: "IsTermsAccepted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TermsVersion",
                table: "users");
        }
    }
}
