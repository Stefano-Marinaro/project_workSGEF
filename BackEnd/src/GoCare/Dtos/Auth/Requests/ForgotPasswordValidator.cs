using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

