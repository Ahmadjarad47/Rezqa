using FluentValidation;
using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Features.Wishlist.Validators;

public class RemoveFromWishlistDtoValidator : AbstractValidator<RemoveFromWishlistDto>
{
    public RemoveFromWishlistDtoValidator()
    {
        RuleFor(x => x.AdId)
            .GreaterThan(0)
            .WithMessage("Ad ID must be greater than 0");
    }
} 