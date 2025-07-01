using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Interfaces;

public interface IWishlistService
{
    Task<WishlistResponseDto> GetWishlistAsync(string userId, CancellationToken cancellationToken = default);
    Task<WishlistResponseDto> AddToWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default);
    Task<WishlistResponseDto> RemoveFromWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default);
    Task<WishlistResponseDto> ClearWishlistAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default);
    Task<int> GetWishlistCountAsync(string userId, CancellationToken cancellationToken = default);
} 