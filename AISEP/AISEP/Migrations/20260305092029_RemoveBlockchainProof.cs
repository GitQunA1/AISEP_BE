using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AISEP.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlockchainProof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_actionlogs_users_UserId",
                table: "actionlogs");

            migrationBuilder.DropForeignKey(
                name: "FK_chatmessages_chatsessions_ChatSessionId",
                table: "chatmessages");

            migrationBuilder.DropForeignKey(
                name: "FK_chatmessages_users_SenderId",
                table: "chatmessages");

            migrationBuilder.DropForeignKey(
                name: "FK_chatsessions_bookings_BookingId",
                table: "chatsessions");

            migrationBuilder.DropForeignKey(
                name: "FK_connectionrequests_investors_InvestorId",
                table: "connectionrequests");

            migrationBuilder.DropForeignKey(
                name: "FK_connectionrequests_startups_StartupId",
                table: "connectionrequests");

            migrationBuilder.DropForeignKey(
                name: "FK_consultingreports_bookings_BookingId",
                table: "consultingreports");

            migrationBuilder.DropForeignKey(
                name: "FK_deals_startups_StartupId",
                table: "deals");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_startups_StartupId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_investor_ai_analyses_startups_StartupId",
                table: "investor_ai_analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_nftrecords_deals_DealId",
                table: "nftrecords");

            migrationBuilder.DropForeignKey(
                name: "FK_postprs_connectionrequests_ConnectionId",
                table: "postprs");

            migrationBuilder.DropForeignKey(
                name: "FK_startup_ai_analyses_startups_StartupId",
                table: "startup_ai_analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_wallettransactions_wallets_WalletId",
                table: "wallettransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_withdrawrequests_wallets_WalletId",
                table: "withdrawrequests");

            migrationBuilder.DropTable(
                name: "blockchainproof");

            migrationBuilder.DropPrimaryKey(
                name: "PK_withdrawrequests",
                table: "withdrawrequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wallettransactions",
                table: "wallettransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_startup_ai_analyses",
                table: "startup_ai_analyses");

            migrationBuilder.DropIndex(
                name: "IX_startup_ai_analyses_StartupId",
                table: "startup_ai_analyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nftrecords",
                table: "nftrecords");

            migrationBuilder.DropIndex(
                name: "IX_nftrecords_DealId",
                table: "nftrecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_consultingreports",
                table: "consultingreports");

            migrationBuilder.DropIndex(
                name: "IX_consultingreports_BookingId",
                table: "consultingreports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_connectionrequests",
                table: "connectionrequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chatsessions",
                table: "chatsessions");

            migrationBuilder.DropIndex(
                name: "IX_chatsessions_BookingId",
                table: "chatsessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chatmessages",
                table: "chatmessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_actionlogs",
                table: "actionlogs");

            migrationBuilder.DropColumn(
                name: "BusinessModel",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "Competitors",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "DevelopmentStage",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "KeySkills",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "MarketSize",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "ProblemStatement",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "Revenue",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "SolutionDescription",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "TargetCustomers",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "TeamExperience",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "TeamMembers",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "UniqueValueProposition",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "actionlogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "actionlogs");

            migrationBuilder.RenameTable(
                name: "withdrawrequests",
                newName: "withdraw_requests");

            migrationBuilder.RenameTable(
                name: "wallettransactions",
                newName: "wallet_transactions");

            migrationBuilder.RenameTable(
                name: "startup_ai_analyses",
                newName: "project_ai_evaluations");

            migrationBuilder.RenameTable(
                name: "nftrecords",
                newName: "nft_records");

            migrationBuilder.RenameTable(
                name: "consultingreports",
                newName: "consulting_reports");

            migrationBuilder.RenameTable(
                name: "connectionrequests",
                newName: "connection_requests");

            migrationBuilder.RenameTable(
                name: "chatsessions",
                newName: "chat_sessions");

            migrationBuilder.RenameTable(
                name: "chatmessages",
                newName: "chat_messages");

            migrationBuilder.RenameTable(
                name: "actionlogs",
                newName: "action_logs");

            migrationBuilder.RenameColumn(
                name: "FullDescription",
                table: "projects",
                newName: "UniqueValueProposition");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "projects",
                newName: "TeamMembers");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "investor_ai_analyses",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_investor_ai_analyses_StartupId",
                table: "investor_ai_analyses",
                newName: "IX_investor_ai_analyses_ProjectId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "documents",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_StartupId",
                table: "documents",
                newName: "IX_documents_ProjectId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "deals",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_deals_StartupId",
                table: "deals",
                newName: "IX_deals_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_withdrawrequests_WalletId",
                table: "withdraw_requests",
                newName: "IX_withdraw_requests_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_wallettransactions_WalletId",
                table: "wallet_transactions",
                newName: "IX_wallet_transactions_WalletId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "project_ai_evaluations",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "StartupId",
                table: "connection_requests",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_connectionrequests_StartupId",
                table: "connection_requests",
                newName: "IX_connection_requests_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_connectionrequests_InvestorId",
                table: "connection_requests",
                newName: "IX_connection_requests_InvestorId");

            migrationBuilder.RenameIndex(
                name: "IX_chatmessages_SenderId",
                table: "chat_messages",
                newName: "IX_chat_messages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_chatmessages_ChatSessionId",
                table: "chat_messages",
                newName: "IX_chat_messages_ChatSessionId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "action_logs",
                newName: "ActorId");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "action_logs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "action_logs",
                newName: "Reason");

            migrationBuilder.RenameIndex(
                name: "IX_actionlogs_UserId",
                table: "action_logs",
                newName: "IX_action_logs_ActorId");

            migrationBuilder.AddColumn<string>(
                name: "PayosOrderCode",
                table: "transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "startups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessLicenseUrl",
                table: "startups",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "reviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessModel",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Competitors",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "DevelopmentStage",
                table: "projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeySkills",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarketSize",
                table: "projects",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProblemStatement",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Revenue",
                table: "projects",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutionDescription",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetCustomers",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamExperience",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "investors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdentityDocumentUrl",
                table: "investors",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "investor_ai_analyses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "advisors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "advisors",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "withdraw_requests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "withdraw_requests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProofImageUrl",
                table: "withdraw_requests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "project_ai_evaluations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "EligibilityReason",
                table: "project_ai_evaluations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEligibleStartup",
                table: "project_ai_evaluations",
                type: "boolean",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "action_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "action_logs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_withdraw_requests",
                table: "withdraw_requests",
                column: "WithdrawRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wallet_transactions",
                table: "wallet_transactions",
                column: "WalletTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_project_ai_evaluations",
                table: "project_ai_evaluations",
                column: "EvaluationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nft_records",
                table: "nft_records",
                column: "NFTRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_consulting_reports",
                table: "consulting_reports",
                column: "ConsultingReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_connection_requests",
                table: "connection_requests",
                column: "ConnectionRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat_sessions",
                table: "chat_sessions",
                column: "ChatSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chat_messages",
                table: "chat_messages",
                column: "ChatMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_action_logs",
                table: "action_logs",
                column: "ActionLogId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_BookingId",
                table: "reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_ai_evaluations_ProjectId",
                table: "project_ai_evaluations",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nft_records_DealId",
                table: "nft_records",
                column: "DealId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consulting_reports_BookingId",
                table: "consulting_reports",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_BookingId",
                table: "chat_sessions",
                column: "BookingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_action_logs_users_ActorId",
                table: "action_logs",
                column: "ActorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_chat_sessions_ChatSessionId",
                table: "chat_messages",
                column: "ChatSessionId",
                principalTable: "chat_sessions",
                principalColumn: "ChatSessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chat_messages_users_SenderId",
                table: "chat_messages",
                column: "SenderId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_chat_sessions_bookings_BookingId",
                table: "chat_sessions",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_connection_requests_investors_InvestorId",
                table: "connection_requests",
                column: "InvestorId",
                principalTable: "investors",
                principalColumn: "InvestorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_connection_requests_projects_ProjectId",
                table: "connection_requests",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_consulting_reports_bookings_BookingId",
                table: "consulting_reports",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_deals_projects_ProjectId",
                table: "deals",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_projects_ProjectId",
                table: "documents",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_investor_ai_analyses_projects_ProjectId",
                table: "investor_ai_analyses",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nft_records_deals_DealId",
                table: "nft_records",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "DealId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_postprs_connection_requests_ConnectionId",
                table: "postprs",
                column: "ConnectionId",
                principalTable: "connection_requests",
                principalColumn: "ConnectionRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_project_ai_evaluations_projects_ProjectId",
                table: "project_ai_evaluations",
                column: "ProjectId",
                principalTable: "projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reviews_bookings_BookingId",
                table: "reviews",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_wallet_transactions_wallets_WalletId",
                table: "wallet_transactions",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_withdraw_requests_wallets_WalletId",
                table: "withdraw_requests",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_action_logs_users_ActorId",
                table: "action_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_chat_sessions_ChatSessionId",
                table: "chat_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_chat_messages_users_SenderId",
                table: "chat_messages");

            migrationBuilder.DropForeignKey(
                name: "FK_chat_sessions_bookings_BookingId",
                table: "chat_sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_connection_requests_investors_InvestorId",
                table: "connection_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_connection_requests_projects_ProjectId",
                table: "connection_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_consulting_reports_bookings_BookingId",
                table: "consulting_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_deals_projects_ProjectId",
                table: "deals");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_projects_ProjectId",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_investor_ai_analyses_projects_ProjectId",
                table: "investor_ai_analyses");

            migrationBuilder.DropForeignKey(
                name: "FK_nft_records_deals_DealId",
                table: "nft_records");

            migrationBuilder.DropForeignKey(
                name: "FK_postprs_connection_requests_ConnectionId",
                table: "postprs");

            migrationBuilder.DropForeignKey(
                name: "FK_project_ai_evaluations_projects_ProjectId",
                table: "project_ai_evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_reviews_bookings_BookingId",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_wallet_transactions_wallets_WalletId",
                table: "wallet_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_withdraw_requests_wallets_WalletId",
                table: "withdraw_requests");

            migrationBuilder.DropIndex(
                name: "IX_reviews_BookingId",
                table: "reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_withdraw_requests",
                table: "withdraw_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_wallet_transactions",
                table: "wallet_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_project_ai_evaluations",
                table: "project_ai_evaluations");

            migrationBuilder.DropIndex(
                name: "IX_project_ai_evaluations_ProjectId",
                table: "project_ai_evaluations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nft_records",
                table: "nft_records");

            migrationBuilder.DropIndex(
                name: "IX_nft_records_DealId",
                table: "nft_records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_consulting_reports",
                table: "consulting_reports");

            migrationBuilder.DropIndex(
                name: "IX_consulting_reports_BookingId",
                table: "consulting_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_connection_requests",
                table: "connection_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat_sessions",
                table: "chat_sessions");

            migrationBuilder.DropIndex(
                name: "IX_chat_sessions_BookingId",
                table: "chat_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chat_messages",
                table: "chat_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_action_logs",
                table: "action_logs");

            migrationBuilder.DropColumn(
                name: "PayosOrderCode",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "BusinessLicenseUrl",
                table: "startups");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "reviews");

            migrationBuilder.DropColumn(
                name: "BusinessModel",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Competitors",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "DevelopmentStage",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "KeySkills",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "MarketSize",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ProblemStatement",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "Revenue",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "SolutionDescription",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TargetCustomers",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "TeamExperience",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "IdentityDocumentUrl",
                table: "investors");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "advisors");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "ProofImageUrl",
                table: "withdraw_requests");

            migrationBuilder.DropColumn(
                name: "EligibilityReason",
                table: "project_ai_evaluations");

            migrationBuilder.DropColumn(
                name: "IsEligibleStartup",
                table: "project_ai_evaluations");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "action_logs");

            migrationBuilder.RenameTable(
                name: "withdraw_requests",
                newName: "withdrawrequests");

            migrationBuilder.RenameTable(
                name: "wallet_transactions",
                newName: "wallettransactions");

            migrationBuilder.RenameTable(
                name: "project_ai_evaluations",
                newName: "startup_ai_analyses");

            migrationBuilder.RenameTable(
                name: "nft_records",
                newName: "nftrecords");

            migrationBuilder.RenameTable(
                name: "consulting_reports",
                newName: "consultingreports");

            migrationBuilder.RenameTable(
                name: "connection_requests",
                newName: "connectionrequests");

            migrationBuilder.RenameTable(
                name: "chat_sessions",
                newName: "chatsessions");

            migrationBuilder.RenameTable(
                name: "chat_messages",
                newName: "chatmessages");

            migrationBuilder.RenameTable(
                name: "action_logs",
                newName: "actionlogs");

            migrationBuilder.RenameColumn(
                name: "UniqueValueProposition",
                table: "projects",
                newName: "FullDescription");

            migrationBuilder.RenameColumn(
                name: "TeamMembers",
                table: "projects",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "investor_ai_analyses",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_investor_ai_analyses_ProjectId",
                table: "investor_ai_analyses",
                newName: "IX_investor_ai_analyses_StartupId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "documents",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_ProjectId",
                table: "documents",
                newName: "IX_documents_StartupId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "deals",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_deals_ProjectId",
                table: "deals",
                newName: "IX_deals_StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_withdraw_requests_WalletId",
                table: "withdrawrequests",
                newName: "IX_withdrawrequests_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_wallet_transactions_WalletId",
                table: "wallettransactions",
                newName: "IX_wallettransactions_WalletId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "startup_ai_analyses",
                newName: "StartupId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "connectionrequests",
                newName: "StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_connection_requests_ProjectId",
                table: "connectionrequests",
                newName: "IX_connectionrequests_StartupId");

            migrationBuilder.RenameIndex(
                name: "IX_connection_requests_InvestorId",
                table: "connectionrequests",
                newName: "IX_connectionrequests_InvestorId");

            migrationBuilder.RenameIndex(
                name: "IX_chat_messages_SenderId",
                table: "chatmessages",
                newName: "IX_chatmessages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_chat_messages_ChatSessionId",
                table: "chatmessages",
                newName: "IX_chatmessages_ChatSessionId");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "actionlogs",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "actionlogs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "ActorId",
                table: "actionlogs",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_action_logs_ActorId",
                table: "actionlogs",
                newName: "IX_actionlogs_UserId");

            migrationBuilder.AddColumn<string>(
                name: "BusinessModel",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Competitors",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevelopmentStage",
                table: "startups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeySkills",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MarketSize",
                table: "startups",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProblemStatement",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Revenue",
                table: "startups",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolutionDescription",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetCustomers",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamExperience",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamMembers",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqueValueProposition",
                table: "startups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "investor_ai_analyses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "startup_ai_analyses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "actionlogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "actionlogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "actionlogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_withdrawrequests",
                table: "withdrawrequests",
                column: "WithdrawRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_wallettransactions",
                table: "wallettransactions",
                column: "WalletTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_startup_ai_analyses",
                table: "startup_ai_analyses",
                column: "EvaluationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nftrecords",
                table: "nftrecords",
                column: "NFTRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_consultingreports",
                table: "consultingreports",
                column: "ConsultingReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_connectionrequests",
                table: "connectionrequests",
                column: "ConnectionRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chatsessions",
                table: "chatsessions",
                column: "ChatSessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chatmessages",
                table: "chatmessages",
                column: "ChatMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_actionlogs",
                table: "actionlogs",
                column: "ActionLogId");

            migrationBuilder.CreateTable(
                name: "blockchainproof",
                columns: table => new
                {
                    BlockchainProofId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    VerificationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchainproof", x => x.BlockchainProofId);
                    table.ForeignKey(
                        name: "FK_blockchainproof_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_startup_ai_analyses_StartupId",
                table: "startup_ai_analyses",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_nftrecords_DealId",
                table: "nftrecords",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_consultingreports_BookingId",
                table: "consultingreports",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_chatsessions_BookingId",
                table: "chatsessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_blockchainproof_DocumentId",
                table: "blockchainproof",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_actionlogs_users_UserId",
                table: "actionlogs",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chatmessages_chatsessions_ChatSessionId",
                table: "chatmessages",
                column: "ChatSessionId",
                principalTable: "chatsessions",
                principalColumn: "ChatSessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chatmessages_users_SenderId",
                table: "chatmessages",
                column: "SenderId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_chatsessions_bookings_BookingId",
                table: "chatsessions",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_connectionrequests_investors_InvestorId",
                table: "connectionrequests",
                column: "InvestorId",
                principalTable: "investors",
                principalColumn: "InvestorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_connectionrequests_startups_StartupId",
                table: "connectionrequests",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_consultingreports_bookings_BookingId",
                table: "consultingreports",
                column: "BookingId",
                principalTable: "bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_deals_startups_StartupId",
                table: "deals",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_startups_StartupId",
                table: "documents",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_investor_ai_analyses_startups_StartupId",
                table: "investor_ai_analyses",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_nftrecords_deals_DealId",
                table: "nftrecords",
                column: "DealId",
                principalTable: "deals",
                principalColumn: "DealId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_postprs_connectionrequests_ConnectionId",
                table: "postprs",
                column: "ConnectionId",
                principalTable: "connectionrequests",
                principalColumn: "ConnectionRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_startup_ai_analyses_startups_StartupId",
                table: "startup_ai_analyses",
                column: "StartupId",
                principalTable: "startups",
                principalColumn: "StartupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_wallettransactions_wallets_WalletId",
                table: "wallettransactions",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_withdrawrequests_wallets_WalletId",
                table: "withdrawrequests",
                column: "WalletId",
                principalTable: "wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
