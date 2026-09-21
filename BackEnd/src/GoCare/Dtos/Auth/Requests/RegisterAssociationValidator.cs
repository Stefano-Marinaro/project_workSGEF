using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class RegisterAssociationValidator : AbstractValidator<RegisterAssociationRequest>
{
    public RegisterAssociationValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("È necessario inserire l'email.")
            .EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("È necessario inserire la password.")
            .MinimumLength(8);
    }
}
