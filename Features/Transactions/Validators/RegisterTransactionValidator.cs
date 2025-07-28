using BankAccountsApi.Features.Transactions.Commands;
using BankAccountsApi.Features.Transactions.Enums;
using FluentValidation;

namespace BankAccountsApi.Features.Transactions.Validators;

public class RegisterTransactionValidator : AbstractValidator<RegisterTransactionCommand>
{
    public RegisterTransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше нуля");

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Код валюты должен содержать 3 символа");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Укажите тип транзакции.")
            .Must(t => t is TransactionType.Credit or TransactionType.Debit)
            .WithMessage("Неверно указан тип транзакции");
    }
    
}