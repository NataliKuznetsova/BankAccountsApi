using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankAccountsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAccrueInterestProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");
            migrationBuilder.Sql(@"
                CREATE OR REPLACE PROCEDURE accrue_interest(account_id UUID)
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    acc RECORD;
                    v_days INTEGER;
                    v_amount NUMERIC(18,2);
                    v_last_date DATE;
                BEGIN
                    SELECT *, COALESCE(last_interest_date, date_opened) INTO acc
                    FROM ""Accounts""
                    WHERE id = account_id;

                    IF acc IS NULL THEN
                        RAISE EXCEPTION 'Account not found: %', account_id;
                    END IF;

                    IF acc.type NOT IN ('Deposit', 'Credit') THEN
                        RETURN;
                    END IF;

                    v_last_date := acc.last_interest_date;
                    IF v_last_date IS NULL THEN
                        v_last_date := acc.date_opened::date;
                    END IF;

                    v_days := current_date - v_last_date;

                    IF v_days <= 0 THEN
                        RETURN;
                    END IF;

                    v_amount := acc.balance * (acc.interest_rate / 100) * (v_days / 365.0);

                    UPDATE ""Accounts""
                    SET balance = balance + v_amount,
                        last_interest_date = current_date
                    WHERE id = account_id;

                    INSERT INTO ""Transactions""(
                        id,
                        accountid,
                        counterpartyaccountid,
                        amount,
                        currency,
                        type,
                        description,
                        date
                    ) VALUES (
                        gen_random_uuid(),
                        account_id,
                        NULL,
                        v_amount,
                        acc.currency,
                        'Credit',
                        'Accrued interest for ' || v_days || ' days',
                        now()
                    );
                END;
                $$;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS accrue_interest(UUID);");
        }
    }
}
