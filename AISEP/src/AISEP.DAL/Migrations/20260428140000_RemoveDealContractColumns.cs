using System;
using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260428140000_RemoveDealContractColumns")]
    public partial class RemoveDealContractColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalTerms",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "ContractPdfUrl",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "EquityPercentage",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "InvestorSignature",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "InvestorSignedAt",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "StartupSignature",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "StartupSignedAt",
                table: "deals");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalTerms",
                table: "deals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "deals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ContractPdfUrl",
                table: "deals",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EquityPercentage",
                table: "deals",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestorSignature",
                table: "deals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvestorSignedAt",
                table: "deals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "deals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartupSignature",
                table: "deals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartupSignedAt",
                table: "deals",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
