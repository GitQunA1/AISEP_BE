using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePostWithPostPr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.CreateTable(
                name: "postprs",
                columns: table => new
                {
                    PostPrId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConnectionId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    StartupApproved = table.Column<bool>(type: "boolean", nullable: false),
                    InvestorApproved = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_postprs", x => x.PostPrId);
                    table.ForeignKey(
                        name: "FK_postprs_connectionrequests_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "connectionrequests",
                        principalColumn: "ConnectionRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_postprs_ConnectionId",
                table: "postprs",
                column: "ConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "postprs");

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConnectionRequestId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    InvestorApproved = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartupApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_posts_connectionrequests_ConnectionRequestId",
                        column: x => x.ConnectionRequestId,
                        principalTable: "connectionrequests",
                        principalColumn: "ConnectionRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_posts_ConnectionRequestId",
                table: "posts",
                column: "ConnectionRequestId");
        }
    }
}
