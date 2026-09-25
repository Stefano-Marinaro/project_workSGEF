using FluentValidation;
using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;
using GoCare.Models.Domain;

namespace GoCare.Dtos.Domain.Validators;

public sealed class CompleteAssociationProfileValidator
    : AbstractValidator<CompleteAssociationProfileRequest>
{
    public CompleteAssociationProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Headquarter)
            .NotNull()
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Phones)
            .NotEmpty()
            .WithMessage("Inserire almeno un numero di telefono.");

        RuleForEach(x => x.Phones)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.CoveredProvinces)
            .NotEmpty()
            .WithMessage("Inserire almeno una provincia coperta.");

        RuleForEach(x => x.CoveredProvinces)
            .Must(ItalianProvinces.IsValid)
            .WithMessage("Sigla di provincia non valida.");
    }
}
