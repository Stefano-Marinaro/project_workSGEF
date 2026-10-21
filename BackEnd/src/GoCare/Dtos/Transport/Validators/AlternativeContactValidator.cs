using FluentValidation;

using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Transport.Validators;

public class AlternativeContactValidator : AbstractValidator<AlternativeContactRequest>
{
    public AlternativeContactValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Il telefono è obbligatorio.")
            .MaximumLength(32)
            .Matches(@"^\+?[0-9][0-9 ()\-]{5,}$").WithMessage("Il numero di telefono non è valido.");

        RuleFor(c => c.Note)
            .MaximumLength(255);
    }
}
