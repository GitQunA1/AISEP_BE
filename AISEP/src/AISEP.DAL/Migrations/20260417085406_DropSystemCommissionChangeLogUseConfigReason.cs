using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DropSystemCommissionChangeLogUseConfigReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_commission_change_logs");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "system_commission_configs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "system_commission_configs");

            migrationBuilder.CreateTable(
                name: "system_commission_change_logs",
                columns: table => new
                {
                    SystemCommissionChangeLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChangedById = table.Column<int>(type: "integer", nullable: false),
                    SystemCommissionConfigId = table.Column<int>(type: "integer", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    NewEffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewEffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    OldEffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OldEffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OldPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
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
        }
    }
}
