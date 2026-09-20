using FluentValidation;

namespace GoCare.Dtos.Auth.Requests;

public sealed class RefreshValidator : AbstractValidator<RefreshRequest>
{
    public RefreshValidator()
    {
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}
