using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserReportEvidenceFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_project_followers_FollowerId_ProjectId",
                table: "project_followers");

            migrationBuilder.CreateTable(
                name: "StartupFollowers",
                columns: table => new
                {
                    StartupFollowerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FollowedId = table.Column<int>(type: "integer", nullable: false),
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupFollowers", x => x.StartupFollowerId);
                    table.ForeignKey(
                        name: "FK_StartupFollowers_startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "startups",
                        principalColumn: "StartupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartupFollowers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StartupFollowers_StartupId",
                table: "StartupFollowers",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupFollowers_UserId",
                table: "StartupFollowers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StartupFollowers");

            migrationBuilder.CreateIndex(
                name: "IX_project_followers_FollowerId_ProjectId",
                table: "project_followers",
                columns: new[] { "FollowerId", "ProjectId" },
                unique: true);
        }
    }
}
