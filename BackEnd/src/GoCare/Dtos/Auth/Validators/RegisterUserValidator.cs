using FluentValidation;
using GoCare.Dtos.Auth.Requests;

namespace GoCare.Dtos.Auth.Validators;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
    }
}
