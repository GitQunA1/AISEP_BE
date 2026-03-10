using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class SyncConnectionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_startup_followers_startups_StartupId",
                table: "startup_followers");

            migrationBuilder.DropForeignKey(
                name: "FK_startup_followers_users_UserId",
                table: "startup_followers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_startup_followers",
                table: "startup_followers");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "connection_requests");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "connection_requests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "connection_requests");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "connection_requests");

            migrationBuilder.DropColumn(
                name: "ResponseMessage",
                table: "connection_requests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "connection_requests");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "startup_followers",
                newName: "FollowerId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "startup_followers",
                newName: "FollowedId");

            migrationBuilder.RenameIndex(
                name: "IX_startup_followers_StartupId",
                table: "startup_followers",
                newName: "IX_startup_followers_FollowerId");

            migrationBuilder.AddColumn<int>(
                name: "StartupFollowerId",
                table: "startup_followers",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "connection_requests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_startup_followers",
                table: "startup_followers",
                column: "StartupFollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_startup_followers_FollowedId",
                table: "startup_followers",
                column: "FollowedId");

            migrationBuilder.AddForeignKey(
                name: "FK_startup_followers_startups_FollowedId",
                table: "startup_followers",
                column: "FollowedId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_startup_followers_users_FollowerId",
                table: "startup_followers",
                column: "FollowerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_startup_followers_startups_FollowedId",
                table: "startup_followers");

            migrationBuilder.DropForeignKey(
                name: "FK_startup_followers_users_FollowerId",
                table: "startup_followers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_startup_followers",
                table: "startup_followers");

            migrationBuilder.DropIndex(
                name: "IX_startup_followers_FollowedId",
                table: "startup_followers");

            migrationBuilder.DropColumn(
                name: "StartupFollowerId",
                table: "startup_followers");

            migrationBuilder.RenameColumn(
                name: "FollowerId",
                table: "startup_followers",
                newName: "StartupId");

            migrationBuilder.RenameColumn(
                name: "FollowedId",
                table: "startup_followers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_startup_followers_FollowerId",
                table: "startup_followers",
                newName: "IX_startup_followers_StartupId");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "connection_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "connection_requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "connection_requests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "connection_requests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "connection_requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "ResponseMessage",
                table: "connection_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "connection_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_startup_followers",
                table: "startup_followers",
                columns: new[] { "UserId", "StartupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_startup_followers_startups_StartupId",
                table: "startup_followers",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_startup_followers_users_UserId",
                table: "startup_followers",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
