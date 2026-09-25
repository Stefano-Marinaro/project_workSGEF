using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class RegisterAssociationValidator : AbstractValidator<RegisterAssociationRequest>
{
    public RegisterAssociationValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("È necessario inserire l'email.")
            .EmailAddress();

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("È necessario inserire la password.")
            .MinimumLength(8);
    }
}
