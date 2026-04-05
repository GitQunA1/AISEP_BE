using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovePostPrApprovalFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""postprs"" DROP COLUMN IF EXISTS ""InvestorApproved"";");
            migrationBuilder.Sql(@"ALTER TABLE ""postprs"" DROP COLUMN IF EXISTS ""StartupApproved"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""postprs"" ADD COLUMN IF NOT EXISTS ""InvestorApproved"" boolean NOT NULL DEFAULT FALSE;");
            migrationBuilder.Sql(@"ALTER TABLE ""postprs"" ADD COLUMN IF NOT EXISTS ""StartupApproved"" boolean NOT NULL DEFAULT FALSE;");
        }
    }
}
