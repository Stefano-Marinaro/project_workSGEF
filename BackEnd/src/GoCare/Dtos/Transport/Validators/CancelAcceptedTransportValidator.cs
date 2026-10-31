using FluentValidation;

using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Transport.Validators;

public sealed class CancelAcceptedTransportValidator : AbstractValidator<CancelAcceptedTransportRequest>
{
    public CancelAcceptedTransportValidator()
    {
        // la causale è obbligatoria: il caregiver deve sapere perché il trasporto è stato disdetto
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Indicare il motivo della disdetta.")
            .MaximumLength(300); // come la colonna Reason di TransportRequestRejection
    }
}
