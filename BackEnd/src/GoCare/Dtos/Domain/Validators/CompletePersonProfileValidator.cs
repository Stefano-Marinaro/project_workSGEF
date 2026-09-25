using FluentValidation;
using GoCare.Dtos.Domain.Requests;

namespace GoCare.Dtos.Domain.Validators;

public sealed class CompletePersonProfileValidator
    : AbstractValidator<CompletePersonProfileRequest>
{
    public CompletePersonProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Surname)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La data di nascita deve essere nel passato.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(32);
    }
}
