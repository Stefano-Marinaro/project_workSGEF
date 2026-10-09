using FluentValidation;

using GoCare.Dtos.Devices.Requests;

namespace GoCare.Dtos.Devices.Validators;

public sealed class RegisterDeviceValidator : AbstractValidator<RegisterDeviceRequest>
{
    public RegisterDeviceValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PushToken)
            .NotEmpty().WithMessage("Il token del dispositivo è obbligatorio.")
            .MaximumLength(255);

        RuleFor(x => x.Platform)
            .IsInEnum().WithMessage("Piattaforma non valida.");
    }
}
