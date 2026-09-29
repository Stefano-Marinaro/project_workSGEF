using FluentValidation;

using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Transport.Validators;

public class CompanionValidator : AbstractValidator<CompanionRequest>
{
    public CompanionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Il nome è obbligatorio.")
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(c => c.Surname)
            .NotEmpty().WithMessage("Il cognome è obbligatorio.")
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(c => c.Relationship)
            .NotEmpty().WithMessage("La parentela è obbligatoria.")
            .MaximumLength(50);

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Il telefono è obbligatorio.")
            .MaximumLength(32)
            .Matches(@"^\+?[0-9][0-9 ()\-]{5,}$").WithMessage("Il numero di telefono non è valido.");
    }
}
