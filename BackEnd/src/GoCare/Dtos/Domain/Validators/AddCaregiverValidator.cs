using FluentValidation;
using GoCare.Dtos.Domain.Requests;

namespace GoCare.Dtos.Domain.Validators;

public sealed class AddCaregiverValidator : AbstractValidator<AddCaregiverRequest>
{
    public AddCaregiverValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(255)
            .EmailAddress().WithMessage("L'email del caregiver non è valida.");
    }
}
