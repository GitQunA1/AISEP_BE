using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvisorBankAccountsAndPayoutBankSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountHolderName",
                table: "monthly_payouts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "monthly_payouts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "monthly_payouts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "advisor_bank_accounts",
                columns: table => new
                {
                    AdvisorBankAccountId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdvisorId = table.Column<int>(type: "integer", nullable: false),
                    BankName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advisor_bank_accounts", x => x.AdvisorBankAccountId);
                    table.ForeignKey(
                        name: "FK_advisor_bank_accounts_advisors_AdvisorId",
                        column: x => x.AdvisorId,
                        principalTable: "advisors",
                        principalColumn: "AdvisorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advisor_bank_accounts_AdvisorId",
                table: "advisor_bank_accounts",
                column: "AdvisorId");

            migrationBuilder.CreateIndex(
                name: "IX_advisor_bank_accounts_AdvisorId_IsActive",
                table: "advisor_bank_accounts",
                columns: new[] { "AdvisorId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advisor_bank_accounts");

            migrationBuilder.DropColumn(
                name: "AccountHolderName",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "monthly_payouts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "monthly_payouts");
        }
    }
}
