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
                CREATE OR REPLACE FUNCTION accrue_interest(account_id UUID)
                RETURNS TABLE(
                    period_from DATE,
                    period_to DATE,
                    amount NUMERIC(18,2)
                )
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    acc RECORD;
                    v_days INTEGER;
                    interest_amount NUMERIC(18,2);
                BEGIN
                    -- Получаем данные аккаунта
                    SELECT *,
                           COALESCE(""LastInterestDate"", ""OpenDate"") AS calc_last_date
                    INTO acc
                    FROM ""Accounts""
                    WHERE ""Id"" = account_id;

                    IF acc IS NULL THEN
                        RAISE EXCEPTION 'Account not found: %', account_id;
                    END IF;

                    -- Проверяем тип счета
                    IF acc.""Type"" NOT IN (1, 3) THEN
                        RETURN;
                    END IF;

                    period_from := acc.calc_last_date::date;
                    period_to := current_date;
                    v_days := period_to - period_from;

                    IF v_days <= 0 THEN
                        RETURN;
                    END IF;

                    -- Считаем процент
                    interest_amount := acc.""Balance"" * COALESCE(acc.""InterestRate"",0) / 100 * (v_days / 365.0);
                    amount := interest_amount;

                    -- Обновляем баланс и дату последнего начисления
                    UPDATE ""Accounts""
                    SET ""Balance"" = ""Balance"" + interest_amount,
                        ""LastInterestDate"" = current_date
                    WHERE ""Id"" = account_id;

                    -- Вставляем транзакцию
                    INSERT INTO ""Transactions""(
                        ""Id"",
                        ""AccountId"",
                        ""TransferId"",
                        ""Amount"",
                        ""Currency"",
                        ""Type"",
                        ""Description"",
                        ""Date""
                    ) VALUES (
                        gen_random_uuid(),
                        account_id,
                        gen_random_uuid(),
                        interest_amount,
                        acc.""Currency"",
                        'Credit',
                        'Accrued interest for ' || v_days || ' days',
                        now()
                    );

                    RETURN NEXT;
                END;
                $$;

            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS accrue_interest(UUID);");
        }
    }
}
