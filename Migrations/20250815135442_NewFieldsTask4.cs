using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAccountsApi.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldsTask4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "OutboxMessages",
                newName: "Type");

            migrationBuilder.AddColumn<Guid>(
                name: "TransferId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedAt",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFailed",
                table: "OutboxMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastInterestDate",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "inbox_consumed",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Handler = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_consumed", x => x.MessageId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_consumed");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "IsFailed",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LastInterestDate",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OutboxMessages",
                newName: "EventType");
        }
    }
}
