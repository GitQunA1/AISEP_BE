using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260430090000_RenameRequiredStageOptionIdsToStageOptionIds")]
    public partial class RenameRequiredStageOptionIdsToStageOptionIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequiredStageOptionIds",
                table: "form_validation_rules",
                newName: "StageOptionIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StageOptionIds",
                table: "form_validation_rules",
                newName: "RequiredStageOptionIds");
        }
    }
}
