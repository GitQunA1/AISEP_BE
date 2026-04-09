using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DropNftArtifacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nft_records");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nft_records",
                columns: table => new
                {
                    NFTRecordId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DealId = table.Column<int>(type: "integer", nullable: false),
                    MintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OwnerWallet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PreviousOwnerWallet = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TokenId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Transferable = table.Column<bool>(type: "boolean", nullable: false),
                    TxHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ValidityStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nft_records", x => x.NFTRecordId);
                    table.ForeignKey(
                        name: "FK_nft_records_deals_DealId",
                        column: x => x.DealId,
                        principalTable: "deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nft_records_DealId",
                table: "nft_records",
                column: "DealId",
                unique: true);
        }
    }
}
