using FluentValidation;
using Rezqa.Application.Features.Category.Requests.Commands;

namespace Rezqa.Application.Features.Category.Validators;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required");

        RuleFor(x => x.Image)
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
            .WithMessage("Image size must not exceed 5MB")
            .Must(file => file == null || 
                file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("File must be an image");
    }
} 