using FluentValidation;

using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class SetPasswordValidator : AbstractValidator<SetPasswordRequest>
{
    public SetPasswordValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Token)
            .NotEmpty();

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("È necessario inserire la password.")
            .MinimumLength(8);
    }
}
