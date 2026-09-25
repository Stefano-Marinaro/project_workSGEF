using FluentValidation;

using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Validators;

public class NewTransportValidator : AbstractValidator<NewTransportRequest>
{
    public NewTransportValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.BeneficiaryId)
            .NotEmpty().WithMessage("L'assistito è obbligatorio.");

        RuleFor(t => t.TripType)
            .IsInEnum();

        RuleFor(t => t.TripDirection)
            .IsInEnum();

        RuleFor(t => t.DepartureDateHour)
            .GreaterThan(_ => DateTimeOffset.UtcNow).WithMessage("La partenza deve essere nel futuro.");

        // stesse regole di TransportRequest.EnsureScheduleConsistency, qui per rispondere 422 invece di 500
        When(t => t.TripDirection == ETripDirection.RoundTrip, () =>
        {
            RuleFor(t => t.ReturnDateHour)
                .NotNull().WithMessage("Per un viaggio di andata e ritorno serve l'orario di ritorno.")
                .GreaterThan(t => t.DepartureDateHour).WithMessage("Il ritorno deve essere successivo alla partenza.");
        }).Otherwise(() =>
        {
            RuleFor(t => t.ReturnDateHour)
                .Null().WithMessage("L'orario di ritorno è ammesso solo per un viaggio di andata e ritorno.");

            RuleFor(t => t.ReturnEndAddress)
                .Null().WithMessage("L'indirizzo di ritorno è ammesso solo per un viaggio di andata e ritorno.");
        });

        RuleFor(t => t.StartAddress)
            .NotNull().WithMessage("L'indirizzo di partenza è obbligatorio.")
            .SetValidator(new AddressValidator());

        RuleFor(t => t.EndAddress)
            .NotNull().WithMessage("L'indirizzo di destinazione è obbligatorio.")
            .SetValidator(new AddressValidator());

        // facoltativo: se assente il ritorno è verso l'indirizzo di partenza
        RuleFor(t => t.ReturnEndAddress)
            .SetValidator(new AddressValidator()!)
            .When(t => t.ReturnEndAddress is not null);

        RuleFor(t => t.ReferencePhone)
            .NotEmpty().WithMessage("Il telefono di riferimento è obbligatorio.")
            .MaximumLength(32)
            .Matches(@"^\+?[0-9][0-9 ()\-]{5,}$").WithMessage("Il numero di telefono non è valido.");

        RuleFor(t => t.ReferenceEmail)
            .NotEmpty().WithMessage("L'email di riferimento è obbligatoria.")
            .MaximumLength(255)
            .EmailAddress().WithMessage("L'email di riferimento non è valida.");

        RuleForEach(t => t.Companions)
            .SetValidator(new CompanionValidator());
    }
}
