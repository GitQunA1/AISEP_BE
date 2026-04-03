using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDealSequentialSigning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractSignedByUserId",
                table: "deals");

            migrationBuilder.RenameColumn(
                name: "ContractSignedAt",
                table: "deals",
                newName: "StartupSignedAt");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalTerms",
                table: "deals",
                type: "text",
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
                name: "StartupSignature",
                table: "deals",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalTerms",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "InvestorSignature",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "InvestorSignedAt",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "StartupSignature",
                table: "deals");

            migrationBuilder.RenameColumn(
                name: "StartupSignedAt",
                table: "deals",
                newName: "ContractSignedAt");

            migrationBuilder.AddColumn<int>(
                name: "ContractSignedByUserId",
                table: "deals",
                type: "integer",
                nullable: true);
        }
    }
}
