using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectionRequestChatSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                table: "chat_sessions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionRequestId",
                table: "chat_sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_ConnectionRequestId",
                table: "chat_sessions",
                column: "ConnectionRequestId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_chat_sessions_context",
                table: "chat_sessions",
                sql: "(\"BookingId\" IS NOT NULL AND \"ConnectionRequestId\" IS NULL) OR (\"BookingId\" IS NULL AND \"ConnectionRequestId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_chat_sessions_connection_requests_ConnectionRequestId",
                table: "chat_sessions",
                column: "ConnectionRequestId",
                principalTable: "connection_requests",
                principalColumn: "ConnectionRequestId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chat_sessions_connection_requests_ConnectionRequestId",
                table: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "IX_chat_sessions_ConnectionRequestId",
                table: "chat_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_chat_sessions_context",
                table: "chat_sessions");

            migrationBuilder.DropColumn(
                name: "ConnectionRequestId",
                table: "chat_sessions");

            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                table: "chat_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
