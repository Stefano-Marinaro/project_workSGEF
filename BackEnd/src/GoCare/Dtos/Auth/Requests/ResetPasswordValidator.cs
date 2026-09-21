using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(p => p.Token)
            .NotEmpty();

        RuleFor(p => p.NewPassword)
            .NotEmpty()
            .MinimumLength(8);
    }
}

