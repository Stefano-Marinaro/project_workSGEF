using FluentValidation;

using GoCare.Data;
using GoCare.Dtos.Transport.Requests;
using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Dtos.Transport.Validators;

public sealed class AddressValidator : AbstractValidator<AddressRequest>
{
    public AddressValidator(GoCareDbContext db)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(a => a.Street)
            .NotEmpty().WithMessage("La via è obbligatoria.")
            .MaximumLength(200);

        RuleFor(a => a.Number)
            .NotEmpty().WithMessage("Il numero civico è obbligatorio.")
            .MaximumLength(20)
            .Matches(@"^(?!0+(?!\d))(\d+[A-Za-z]*([/\- ][A-Za-z0-9]+){0,2}|[Ss][Nn][Cc])$")
                .WithMessage("Il numero civico non è valido.");

        RuleFor(a => a.PostalCode)
            .NotEmpty().WithMessage("Il CAP è obbligatorio.")
            .Matches(@"^\d{5}$").WithMessage("Il CAP deve essere di 5 cifre.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("Il comune è obbligatorio.")
            .MaximumLength(120);

        RuleFor(a => a.Province)
            .NotEmpty().WithMessage("La provincia è obbligatoria.")
            .Must(ItalianProvinces.IsValid).WithMessage("Sigla di provincia non valida.");

        // Il comune deve esistere davvero, non solo avere il formato giusto: l'autocomplete
        // lato frontend non basta, un client potrebbe mandare qualunque stringa.
        RuleFor(a => a).MustAsync(async (address, ct) =>
        {
            var comune = await db.Comuni.SingleOrDefaultAsync(
                c => c.NomeComune == address.City && c.Provincia == address.Province, ct);

            if (comune is null)
                return false;

            // CapCodes vuoto = comune non ancora catalogato per il CAP: non blocchiamo,
            // meglio accettare un CAP scritto a mano che rifiutare un comune valido.
            if (comune.CapCodes.Count > 0 && !comune.CapCodes.Contains(address.PostalCode))
                return false;

            return true;
        }).WithMessage("Comune, provincia o CAP non coerenti tra loro.");
    }
}
