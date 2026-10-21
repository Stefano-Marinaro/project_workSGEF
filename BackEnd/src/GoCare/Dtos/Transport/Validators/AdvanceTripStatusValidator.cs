using FluentValidation;
using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Transport.Validators;

public sealed class AdvanceTripStatusValidator : AbstractValidator<AdvanceTripStatusRequest>
{
    public AdvanceTripStatusValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.OperatorLabel)
            .NotEmpty().WithMessage("Il nome dell'operatore è obbligatorio.")
            .MaximumLength(200);
    }
}
