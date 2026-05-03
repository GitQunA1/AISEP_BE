using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultScorecardWeightConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "scorecard_weight_configs",
                columns: new[] { "Id", "CompetitionWeight", "ConfigName", "CreatedAt", "IndustryId", "InvestmentNeedWeight", "MarketWeight", "ProductWeight", "StageId", "TeamWeight", "TractionWeight" },
                values: new object[] { 1, 10.0m, "Default Bill Payne Method", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 10.0m, 25.0m, 15.0m, null, 30.0m, 10.0m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "scorecard_weight_configs",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
