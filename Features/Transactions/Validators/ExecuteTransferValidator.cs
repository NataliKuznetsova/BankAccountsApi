using BankAccountsApi.Features.Transactions.Commands;
using FluentValidation;

namespace BankAccountsApi.Features.Transactions.Validators;

/// <summary>
/// Валидация перевода денег
/// </summary>
public class ExecuteTransferValidator : AbstractValidator<ExecuteTransferCommand>
{
    public ExecuteTransferValidator()
    {
        RuleFor(x => x.FromAccountId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Не указан Отправитель");

        RuleFor(x => x.ToAccountId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Не указан получатель");

        RuleFor(x => x.Amount)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0");

        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Валюта обязательна.")
            .Length(3).WithMessage("Валюта должна быть 3 символа");
    }
}