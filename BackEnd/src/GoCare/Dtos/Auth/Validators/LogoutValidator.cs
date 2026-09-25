using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class LogoutValidator : AbstractValidator<LogoutRequest>
{
    public LogoutValidator()
    {
        RuleFor(l => l.RefreshToken)
            .NotEmpty();
    }
}

