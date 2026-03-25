using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceFieldsToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"transactions\" ADD COLUMN IF NOT EXISTS \"ReferenceId\" integer;");
            migrationBuilder.Sql("ALTER TABLE \"transactions\" ADD COLUMN IF NOT EXISTS \"ReferenceType\" character varying(50);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"transactions\" DROP COLUMN IF EXISTS \"ReferenceId\";");
            migrationBuilder.Sql("ALTER TABLE \"transactions\" DROP COLUMN IF EXISTS \"ReferenceType\";");
        }
    }
}
