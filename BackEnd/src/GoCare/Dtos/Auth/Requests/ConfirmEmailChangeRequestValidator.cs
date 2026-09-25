using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class ConfirmEmailChangeRequestValidator
    : AbstractValidator<ConfirmEmailChangeRequest>
{
    public ConfirmEmailChangeRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}
