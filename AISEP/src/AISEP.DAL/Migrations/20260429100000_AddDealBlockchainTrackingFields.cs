using System;
using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260429100000_AddDealBlockchainTrackingFields")]
    public partial class AddDealBlockchainTrackingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "deals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockchainTxHash",
                table: "deals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BlockchainVerifiedAt",
                table: "deals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockchainErrorMessage",
                table: "deals",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockchainErrorMessage",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "BlockchainVerifiedAt",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "BlockchainTxHash",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "deals");
        }
    }
}
