using AISEP.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISEP.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260428123000_RefactorDealThreeStepFlow")]
    public partial class RefactorDealThreeStepFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "deals",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitiatorRole",
                table: "deals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Investor");

            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'PendingCounterpartyConfirmation' WHERE \"Status\" = 'Pending';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'PendingStaffApproval' WHERE \"Status\" = 'Confirmed';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'PendingStaffApproval' WHERE \"Status\" = 'Waiting_For_Startup_Signature';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Completed' WHERE \"Status\" = 'Contract_Signed';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Canceled' WHERE \"Status\" = 'Rejected';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'BlockchainFailed' WHERE \"Status\" = 'Failed';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Pending' WHERE \"Status\" = 'PendingCounterpartyConfirmation';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Confirmed' WHERE \"Status\" = 'PendingStaffApproval';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Pending' WHERE \"Status\" = 'RequireReupload';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Confirmed' WHERE \"Status\" = 'ProcessingBlockchain';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Contract_Signed' WHERE \"Status\" = 'Completed';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Rejected' WHERE \"Status\" = 'Canceled';");
            migrationBuilder.Sql("UPDATE \"deals\" SET \"Status\" = 'Failed' WHERE \"Status\" = 'BlockchainFailed';");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "deals");

            migrationBuilder.DropColumn(
                name: "InitiatorRole",
                table: "deals");
        }
    }
}
