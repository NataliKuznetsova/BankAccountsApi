using BankAccountsApi.Features.Account.Commands;
using FluentValidation;

namespace BankAccountsApi.Features.Account.Validators;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Поле тип счета обязательно")
            .Must(type => type == "Checking" || type == "Deposit" || type == "Credit")
            .WithMessage("Неверное значение поля тип счета");
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Поле Валюта обязательно")
            .Length(3).WithMessage("Валюта должна быть в формате ISO 4217");
    }
}