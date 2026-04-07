using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class WalletCommissionFlow_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SystemCommissionAmount",
                table: "bookings",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SystemCommissionConfigId",
                table: "bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SystemCommissionPercent",
                table: "bookings",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "system_commission_configs",
                columns: table => new
                {
                    SystemCommissionConfigId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_commission_configs", x => x.SystemCommissionConfigId);
                    table.CheckConstraint("CK_system_commission_configs_effective_range", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_system_commission_configs_percent_range", "\"Percent\" >= 0 AND \"Percent\" <= 100");
                    table.ForeignKey(
                        name: "FK_system_commission_configs_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "system_commission_change_logs",
                columns: table => new
                {
                    SystemCommissionChangeLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SystemCommissionConfigId = table.Column<int>(type: "integer", nullable: true),
                    OldPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    NewPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    OldEffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OldEffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewEffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewEffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedById = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_commission_change_logs", x => x.SystemCommissionChangeLogId);
                    table.ForeignKey(
                        name: "FK_system_commission_change_logs_system_commission_configs_Sys~",
                        column: x => x.SystemCommissionConfigId,
                        principalTable: "system_commission_configs",
                        principalColumn: "SystemCommissionConfigId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_system_commission_change_logs_users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_SystemCommissionConfigId",
                table: "bookings",
                column: "SystemCommissionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_system_commission_change_logs_ChangedAt",
                table: "system_commission_change_logs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_system_commission_change_logs_ChangedById",
                table: "system_commission_change_logs",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_system_commission_change_logs_SystemCommissionConfigId",
                table: "system_commission_change_logs",
                column: "SystemCommissionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_system_commission_configs_CreatedById",
                table: "system_commission_configs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_system_commission_configs_IsActive",
                table: "system_commission_configs",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = TRUE");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_system_commission_configs_SystemCommissionConfigId",
                table: "bookings",
                column: "SystemCommissionConfigId",
                principalTable: "system_commission_configs",
                principalColumn: "SystemCommissionConfigId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_system_commission_configs_SystemCommissionConfigId",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "system_commission_change_logs");

            migrationBuilder.DropTable(
                name: "system_commission_configs");

            migrationBuilder.DropIndex(
                name: "IX_bookings_SystemCommissionConfigId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "SystemCommissionAmount",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "SystemCommissionConfigId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "SystemCommissionPercent",
                table: "bookings");
        }
    }
}
