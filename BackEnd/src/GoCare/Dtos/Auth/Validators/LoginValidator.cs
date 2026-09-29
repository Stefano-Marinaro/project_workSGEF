using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("L'email è obbligatoria")
            .EmailAddress().WithMessage("L'email non è valida");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("La pasword è obbligatoria.");
    }
}
