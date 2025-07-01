using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.Wishlist.Dtos;

public record AddToWishlistDto
{
    [Required]
    public int AdId { get; init; }
}

public record RemoveFromWishlistDto
{
    [Required]
    public int AdId { get; init; }
}

public record WishlistItemDto
{
    public int AdId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string[]? ImageUrl { get; init; }
    public string Location { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string SubCategoryName { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
}

public record WishlistResponseDto
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<WishlistItemDto>? Items { get; init; }
    public int TotalCount { get; init; }
} 