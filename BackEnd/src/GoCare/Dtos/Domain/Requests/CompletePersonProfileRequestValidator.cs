using FluentValidation;

namespace GoCare.Dtos.Domain.Requests;

public sealed class CompletePersonProfileRequestValidator
    : AbstractValidator<CompletePersonProfileRequest>
{
    public CompletePersonProfileRequestValidator()
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
