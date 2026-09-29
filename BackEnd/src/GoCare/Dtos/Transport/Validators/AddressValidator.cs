using FluentValidation;

using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Domain;

namespace GoCare.Dtos.Transport.Validators;

public sealed class AddressValidator : AbstractValidator<AddressRequest>
{
    public AddressValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(a => a.Street)
            .NotEmpty().WithMessage("La via è obbligatoria.")
            .MaximumLength(200);

        RuleFor(a => a.Number)
            .NotEmpty().WithMessage("Il numero civico è obbligatorio.")
            .MaximumLength(20)
            .Matches(@"^(?!0+(?!\d))(\d+[A-Za-z]*([/\- ][A-Za-z0-9]+){0,2}|[Ss][Nn][Cc])$")
                .WithMessage("Il numero civico non è valido.");

        RuleFor(a => a.PostalCode)
            .NotEmpty().WithMessage("Il CAP è obbligatorio.")
            .Matches(@"^\d{5}$").WithMessage("Il CAP deve essere di 5 cifre.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("Il comune è obbligatorio.")
            .MaximumLength(120);

        RuleFor(a => a.Province)
            .NotEmpty().WithMessage("La provincia è obbligatoria.")
            .Must(ItalianProvinces.IsValid).WithMessage("Sigla di provincia non valida.");
    }
}
