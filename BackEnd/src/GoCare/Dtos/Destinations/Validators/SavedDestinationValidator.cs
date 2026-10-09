using FluentValidation;

using GoCare.Dtos.Destinations.Requests;
using GoCare.Dtos.Transport.Validators;

namespace GoCare.Dtos.Destinations.Validators;

public sealed class SavedDestinationValidator : AbstractValidator<SavedDestinationRequest>
{
    public SavedDestinationValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PlaceName)
            .NotEmpty().WithMessage("Il nome del luogo è obbligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.SavedAddress)
            .NotNull()
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Note)
            .MaximumLength(255);
    }
}
