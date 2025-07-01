using FluentValidation;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Validators;

public class CreateAdDtoValidator : AbstractValidator<CreateAdDto>
{
    public CreateAdDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .Length(5, 200).WithMessage("Title must be between 5 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .Length(10, 2000).WithMessage("Description must be between 10 and 2000 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");


        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}