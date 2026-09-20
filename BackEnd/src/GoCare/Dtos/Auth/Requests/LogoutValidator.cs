using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class LogoutValidator : AbstractValidator<LogoutRequest>
{
    public LogoutValidator()
    {
        RuleFor(l => l.RefreshToken)
            .NotEmpty();
    }
}

