using FluentValidation;

using GoCare.Data;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Enums;

namespace GoCare.Dtos.Transport.Validators;

public class NewTransportValidator : AbstractValidator<NewTransportRequest>
{
    public NewTransportValidator(GoCareDbContext db)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.BeneficiaryId)
            .NotEmpty().WithMessage("L'assistito è obbligatorio.");

        RuleFor(t => t.TripType)
            .IsInEnum();

        RuleFor(t => t.TripDirection)
            .IsInEnum();

        // stessa regola di TransportRequest.EnsureScheduleConsistency, qui per rispondere 422 invece di 500
        When(t => t.TripType == ETripType.Hospitalization || t.TripType == ETripType.Discharge, () =>
        {
            RuleFor(t => t.TripDirection)
                .Equal(ETripDirection.OneWay).WithMessage("Ricovero e Dimissione ammettono solo un viaggio di sola andata/ritorno (OneWay), non Andata e ritorno.");
        });

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
            .SetValidator(new AddressValidator(db));

        RuleFor(t => t.EndAddress)
            .NotNull().WithMessage("L'indirizzo di destinazione è obbligatorio.")
            .SetValidator(new AddressValidator(db));

        // facoltativo: se assente il ritorno è verso l'indirizzo di partenza
        RuleFor(t => t.ReturnEndAddress)
            .SetValidator(new AddressValidator(db)!)
            .When(t => t.ReturnEndAddress is not null);

        RuleFor(t => t.ReferencePhone)
            .NotEmpty().WithMessage("Il telefono di riferimento è obbligatorio.")
            .MaximumLength(32)
            .Matches(@"^\+?[0-9][0-9 ()\-]{5,}$").WithMessage("Il numero di telefono non è valido.");

        RuleFor(t => t.ReferenceEmail)
            .MaximumLength(255)
            .EmailAddress().WithMessage("L'email di riferimento non è valida.")
            .When(t => !string.IsNullOrEmpty(t.ReferenceEmail));

        RuleForEach(t => t.Companions)
            .SetValidator(new CompanionValidator());
    }
}
