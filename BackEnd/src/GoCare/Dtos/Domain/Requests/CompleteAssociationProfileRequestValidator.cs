using FluentValidation;

namespace GoCare.Dtos.Domain.Requests;

public sealed class CompleteAssociationProfileRequestValidator
    : AbstractValidator<CompleteAssociationProfileRequest>
{
    public CompleteAssociationProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Headquarter)
            .NotNull()
            .SetValidator(new AddressRequestValidator());

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
            .NotEmpty()
            .MaximumLength(120);
    }
}

internal sealed class AddressRequestValidator : AbstractValidator<AddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(120);
    }
}
