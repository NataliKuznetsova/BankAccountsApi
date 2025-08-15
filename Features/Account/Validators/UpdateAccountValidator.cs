using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using FluentValidation;

namespace BankAccountsApi.Features.Account.Validators;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Поле тип счета обязательно")
            .Must(type => type == AccountType.Checking || type == AccountType.Deposit || type == AccountType.Credit)
            .WithMessage("Неверное значение поля тип счета");
        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Поле Валюта обязательно")
            .Length(3).WithMessage("Валюта должна быть в формате ISO 4217");
    }
}