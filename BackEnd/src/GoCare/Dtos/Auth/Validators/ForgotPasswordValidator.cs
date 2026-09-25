using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

