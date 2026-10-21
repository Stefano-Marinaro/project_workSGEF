using FluentValidation;

using GoCare.Data;
using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;
using GoCare.Models.Domain;

namespace GoCare.Dtos.Domain.Validators;

public sealed class UpdateAssociationProfileValidator : AbstractValidator<UpdateAssociationProfileRequest>
{
    public UpdateAssociationProfileValidator(GoCareDbContext db)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Headquarter)
            .NotNull()
            .SetValidator(new AddressValidator(db));

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

        RuleFor(x => x.AvailabilityHours)
            .MaximumLength(500)
            .When(x => x.AvailabilityHours is not null);
    }
}