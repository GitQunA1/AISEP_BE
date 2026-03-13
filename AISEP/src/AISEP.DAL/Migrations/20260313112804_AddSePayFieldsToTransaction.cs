using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class AddSePayFieldsToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCode",
                table: "transactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentContent",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SepayTransactionId",
                table: "transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_PaymentCode",
                table: "transactions",
                column: "PaymentCode",
                unique: true,
                filter: "\"PaymentCode\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_PaymentCode",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentCode",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PaymentContent",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "SepayTransactionId",
                table: "transactions");
        }
    }
}
