using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("L'email è obbligatoria")
            .EmailAddress().WithMessage("L'email non è valida");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("La pasword è obbligatoria.");
    }
}
