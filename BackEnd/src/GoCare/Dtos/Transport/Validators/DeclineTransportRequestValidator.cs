using FluentValidation;

using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Transport.Validators;

public sealed class DeclineTransportRequestValidator : AbstractValidator<DeclineTransportRequestRequest>
{
    public DeclineTransportRequestValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(300)
            .When(x => x.Reason is not null);
    }
}
