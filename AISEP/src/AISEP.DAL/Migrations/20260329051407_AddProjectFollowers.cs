using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "startup_followers");

            migrationBuilder.CreateTable(
                name: "project_followers",
                columns: table => new
                {
                    ProjectFollowerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_followers", x => x.ProjectFollowerId);
                    table.ForeignKey(
                        name: "FK_project_followers_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_followers_users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_followers_FollowerId_ProjectId",
                table: "project_followers",
                columns: new[] { "FollowerId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_followers_ProjectId",
                table: "project_followers",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_followers");

            migrationBuilder.CreateTable(
                name: "startup_followers",
                columns: table => new
                {
                    StartupFollowerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowedId = table.Column<int>(type: "integer", nullable: false),
                    FollowerId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_startup_followers", x => x.StartupFollowerId);
                    table.ForeignKey(
                        name: "FK_startup_followers_startups_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "startups",
                        principalColumn: "StartupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_startup_followers_users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_startup_followers_FollowedId",
                table: "startup_followers",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_startup_followers_FollowerId",
                table: "startup_followers",
                column: "FollowerId");
        }
    }
}
