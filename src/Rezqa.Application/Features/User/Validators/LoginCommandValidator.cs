using FluentValidation;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommandRequestDTO>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}