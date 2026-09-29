using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class ResendVerificationEmailValidator
    : AbstractValidator<ResendVerificationEmailRequest>
{
    public ResendVerificationEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
