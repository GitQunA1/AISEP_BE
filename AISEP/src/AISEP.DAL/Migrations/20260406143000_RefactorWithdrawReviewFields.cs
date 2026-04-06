using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    public partial class RefactorWithdrawReviewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "withdraw_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE withdraw_requests
                SET ""ApprovedAt"" = ""ReviewedAt"",
                    ""ApprovedById"" = ""ReviewedById""
                WHERE ""Status"" = 'Approved';
            ");

            migrationBuilder.Sql(@"
                UPDATE withdraw_requests
                SET ""RejectedAt"" = ""ReviewedAt"",
                    ""RejectedById"" = ""ReviewedById"",
                    ""RejectionReason"" = ""ReviewReason""
                WHERE ""Status"" = 'Rejected';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ApprovedById",
                table: "withdraw_requests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_RejectedById",
                table: "withdraw_requests",
                column: "RejectedById");

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ApprovedById",
                table: "withdraw_requests",
                column: "ApprovedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_RejectedById",
                table: "withdraw_requests",
                column: "RejectedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "withdraw_requests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "withdraw_requests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "withdraw_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedById",
                table: "withdraw_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE withdraw_requests
                SET ""ReviewedAt"" = COALESCE(""ApprovedAt"", ""RejectedAt""),
                    ""ReviewedById"" = COALESCE(""ApprovedById"", ""RejectedById""),
                    ""ReviewReason"" = ""RejectionReason"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_withdraw_requests_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_users_ReviewedById",
                table: "withdraw_requests",
                column: "ReviewedById",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_users_RejectedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_withdraw_requests_RejectedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "RejectedById",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "withdraw_requests");
        }
    }
}
