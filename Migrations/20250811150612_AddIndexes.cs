using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAccountsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_accounts_ownerid_hash ON \"Accounts\" USING HASH (\"OwnerId\");");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_transactions_accountid_date ON \"Transactions\" (\"AccountId\", \"Date\");");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS idx_transactions_date_gist ON \"Transactions\" USING GIST (\"Date\");");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_accounts_ownerid_hash;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_transactions_accountid_date;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_transactions_date_gist;");

        }
    }
}
