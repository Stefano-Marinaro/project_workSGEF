using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailValidator()
    {
        RuleFor(r  => r.Token).NotEmpty();
    }
}
