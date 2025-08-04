using BankAccountsApi.Features.Account.Commands;
using BankAccountsApi.Features.Account.Enums;
using FluentValidation;

namespace BankAccountsApi.Features.Account.Validators;

/// <summary>
/// Валидатор для CreateAccountCommand
/// </summary>
public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.OwnerId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Поле OwnerId обязательно");
        RuleFor(x => x.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Поле тип счета обязательно")
            .Must(type => type is AccountType.Checking or AccountType.Deposit or AccountType.Credit)
            .WithMessage("Неверное значение поля тип счета");
        RuleFor(x => x.Currency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Поле Валюта обязательно")
            .Length(3).WithMessage("Валюта должна быть в формате ISO 4217");
    }
}