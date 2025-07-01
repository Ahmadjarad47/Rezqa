using FluentValidation;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Validators;

public class UpdateAdDtoValidator : AbstractValidator<UpdateAdDto>
{
    public UpdateAdDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 2000).WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .Length(1, 100).WithMessage("Location must be between 1 and 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be valid");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");
    }
} 