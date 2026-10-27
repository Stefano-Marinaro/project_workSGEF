using FluentValidation;

using GoCare.Data;
using GoCare.Dtos.Domain.Requests;
using GoCare.Dtos.Transport.Validators;
using Microsoft.EntityFrameworkCore;

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

        RuleFor(x => x.CoveredComuni)
            .NotEmpty()
            .WithMessage("Inserire almeno un comune coperto.");

        RuleForEach(x => x.CoveredComuni)
            .Matches(@"^\d{6}$")
            .WithMessage("Codice ISTAT non valido.");

        RuleFor(x => x.CoveredComuni)
            .Cascade(CascadeMode.Stop)   // al primo errore si ferma: con duplicati non serve controllare l'esistenza
            .Must(codes => codes.Distinct().Count() == codes.Count).WithMessage("Comune ripetuto.")
            .WithMessage("Comune ripetuto.")
            // tutti i codici devono esistere nella tabella comuni: una sola query per tutta la lista
            .MustAsync(async (codes, ct) =>
                await db.Comuni.CountAsync(c => codes.Contains(c.CodiceIstat), ct) == codes.Count)
            .WithMessage("Comune non trovato.")
            .When(x => x.CoveredComuni is { Count: > 0 });

        RuleFor(x => x.AvailabilityHours)
            .MaximumLength(500)
            .When(x => x.AvailabilityHours is not null);
    }
}