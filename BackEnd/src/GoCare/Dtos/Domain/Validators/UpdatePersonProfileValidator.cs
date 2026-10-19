using FluentValidation;

using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;

namespace GoCare.Dtos.Transport.Validators;

public sealed class UpdatePersonProfileValidator : AbstractValidator<UpdatePersonProfileRequest>
{
    public UpdatePersonProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);

        RuleFor(x => x.BirthDate)
        .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
        .WithMessage("La data di nascita deve essere nel passato.");

        RuleFor(x => x.Phone).NotEmpty().MaximumLength(32);

        RuleFor(x => x.HomeAddress!)
        .SetValidator(new AddressValidator())
        .When(x => x.HomeAddress is not null);
    }
}