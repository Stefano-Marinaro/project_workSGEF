using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class ResendVerificationEmailRequestValidator
    : AbstractValidator<ResendVerificationEmailRequest>
{
    public ResendVerificationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
