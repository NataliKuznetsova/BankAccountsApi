using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAccountsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNewChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "inbox_consumed",
                newName: "ConsumedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "Clients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "inbox_dead_letters",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    handler = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_dead_letters", x => x.message_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_dead_letters");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "ConsumedAt",
                table: "inbox_consumed",
                newName: "ProcessedAt");
        }
    }
}
