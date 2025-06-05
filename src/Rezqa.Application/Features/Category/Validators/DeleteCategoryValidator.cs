using FluentValidation;
using Rezqa.Application.Features.Category.Requests.Commands;

namespace Rezqa.Application.Features.Category.Validators;

public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryRequest>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
    }
} 