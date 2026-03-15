using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class InitRejectFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "startups",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "startups",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "investors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "investors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "investors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "investors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "investors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "investors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "advisors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "advisors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "advisors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "advisors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "advisors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "advisors",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_startups_ApprovedById",
                table: "startups",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_startups_CreatedBy",
                table: "startups",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_startups_RejectedById",
                table: "startups",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_investors_ApprovedById",
                table: "investors",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_investors_CreatedBy",
                table: "investors",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_investors_RejectedById",
                table: "investors",
                column: "RejectedById");

            migrationBuilder.CreateIndex(
                name: "IX_advisors_ApprovedById",
                table: "advisors",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_advisors_CreatedBy",
                table: "advisors",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_advisors_RejectedById",
                table: "advisors",
                column: "RejectedById");

            migrationBuilder.AddForeignKey(
                name: "FK_advisors_users_ApprovedById",
                table: "advisors",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_advisors_users_CreatedBy",
                table: "advisors",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_advisors_users_RejectedById",
                table: "advisors",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_investors_users_ApprovedById",
                table: "investors",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_investors_users_CreatedBy",
                table: "investors",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_investors_users_RejectedById",
                table: "investors",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_startups_users_ApprovedById",
                table: "startups",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_startups_users_CreatedBy",
                table: "startups",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_startups_users_RejectedById",
                table: "startups",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_advisors_users_ApprovedById",
                table: "advisors");

            migrationBuilder.DropForeignKey(
                name: "FK_advisors_users_CreatedBy",
                table: "advisors");

            migrationBuilder.DropForeignKey(
                name: "FK_advisors_users_RejectedById",
                table: "advisors");

            migrationBuilder.DropForeignKey(
                name: "FK_investors_users_ApprovedById",
                table: "investors");

            migrationBuilder.DropForeignKey(
                name: "FK_investors_users_CreatedBy",
                table: "investors");

            migrationBuilder.DropForeignKey(
                name: "FK_investors_users_RejectedById",
                table: "investors");

            migrationBuilder.DropForeignKey(
                name: "FK_startups_users_ApprovedById",
                table: "startups");

            migrationBuilder.DropForeignKey(
                name: "FK_startups_users_CreatedBy",
                table: "startups");

            migrationBuilder.DropForeignKey(
                name: "FK_startups_users_RejectedById",
                table: "startups");

            migrationBuilder.DropIndex(
                name: "IX_startups_ApprovedById",
                table: "startups");

            migrationBuilder.DropIndex(
                name: "IX_startups_CreatedBy",
                table: "startups");

            migrationBuilder.DropIndex(
                name: "IX_startups_RejectedById",
                table: "startups");

            migrationBuilder.DropIndex(
                name: "IX_investors_ApprovedById",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_investors_CreatedBy",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_investors_RejectedById",
                table: "investors");

            migrationBuilder.DropIndex(
                name: "IX_advisors_ApprovedById",
                table: "advisors");

            migrationBuilder.DropIndex(
                name: "IX_advisors_CreatedBy",
                table: "advisors");

            migrationBuilder.DropIndex(
                name: "IX_advisors_RejectedById",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "advisors");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "startups",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
