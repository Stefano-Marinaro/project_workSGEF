using FluentValidation;
using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;

namespace GoCare.Dtos.Domain.Validators;

public sealed class AssistedPersonValidator : AbstractValidator<AssistedPersonRequest>
{
    public AssistedPersonValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Il nome è obbligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Il cognome è obbligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("La data di nascita è obbligatoria.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La data di nascita deve essere nel passato.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Il telefono è obbligatorio.")
            .MaximumLength(32)
            .Matches(@"^\+?[0-9][0-9 ()\-]{5,}$")
            .WithMessage("Il numero di telefono non è valido.");

        RuleFor(x => x.HomeAddress)
            .SetValidator(new AddressValidator()!)
            .When(x => x.HomeAddress is not null);
    }
}
