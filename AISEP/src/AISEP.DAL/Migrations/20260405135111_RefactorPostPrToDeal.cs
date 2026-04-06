using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPostPrToDeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_postprs_connection_requests_ConnectionId",
                table: "postprs");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "postprs",
                newName: "DealId");

            migrationBuilder.RenameIndex(
                name: "IX_postprs_ConnectionId",
                table: "postprs",
                newName: "IX_postprs_DealId");

            migrationBuilder.Sql(@"
                UPDATE ""postprs"" AS p
                SET ""DealId"" = m.""DealId""
                FROM (
                    SELECT cr.""ConnectionRequestId"", MIN(d.""DealId"") AS ""DealId""
                    FROM ""connection_requests"" AS cr
                    JOIN ""deals"" AS d
                        ON d.""InvestorId"" = cr.""InvestorId""
                        AND d.""ProjectId"" = cr.""ProjectId""
                    GROUP BY cr.""ConnectionRequestId""
                ) AS m
                WHERE p.""DealId"" = m.""ConnectionRequestId"";
            ");

            migrationBuilder.Sql(@"
                DELETE FROM ""postprs"" AS p
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ""deals"" AS d
                    WHERE d.""DealId"" = p.""DealId""
                );
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_postprs_deals_DealId",
                table: "postprs",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "DealId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_postprs_deals_DealId",
                table: "postprs");

            migrationBuilder.RenameColumn(
                name: "DealId",
                table: "postprs",
                newName: "ConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_postprs_DealId",
                table: "postprs",
                newName: "IX_postprs_ConnectionId");

            migrationBuilder.Sql(@"
                UPDATE ""postprs"" AS p
                SET ""ConnectionId"" = m.""ConnectionRequestId""
                FROM (
                    SELECT d.""DealId"", MIN(cr.""ConnectionRequestId"") AS ""ConnectionRequestId""
                    FROM ""deals"" AS d
                    JOIN ""connection_requests"" AS cr
                        ON cr.""InvestorId"" = d.""InvestorId""
                        AND cr.""ProjectId"" = d.""ProjectId""
                    GROUP BY d.""DealId""
                ) AS m
                WHERE p.""ConnectionId"" = m.""DealId"";
            ");

            migrationBuilder.Sql(@"
                DELETE FROM ""postprs"" AS p
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ""connection_requests"" AS cr
                    WHERE cr.""ConnectionRequestId"" = p.""ConnectionId""
                );
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_postprs_connection_requests_ConnectionId",
                table: "postprs",
                column: "ConnectionId",
                principalTable: "connection_requests",
                principalColumn: "ConnectionRequestId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
