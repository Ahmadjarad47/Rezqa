using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Interfaces;
using Rezqa.Infrastructure.Persistence;

namespace Rezqa.Infrastructure.Persistence.Repositories;

public class WishlistRepository : IRepository<WishlistItemDto>, IWishlistRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WishlistRepository> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(1);

    public WishlistRepository(
        IMemoryCache memoryCache,
        ApplicationDbContext context,
        ILogger<WishlistRepository> logger)
    {
        _memoryCache = memoryCache;
        _context = context;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> GetWishlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            var wishlistItems = GetWishlistFromCache(cacheKey);

            if (!wishlistItems.Any())
            {
                return new WishlistResponseDto
                {
                    IsSuccess = true,
                    Message = "Your wishlist is empty.",
                    Items = new List<WishlistItemDto>(),
                    TotalCount = 0
                };
            }

            // Get ad IDs from wishlist
            var adIds = wishlistItems.Select(item => item.AdId).ToList();

            // Fetch full ad details from database
            var ads = await _context.ads
                .Include(a => a.Category)
                .Include(a => a.SubCategory)
                .Where(a => adIds.Contains(a.Id) && a.isActive)
                .ToListAsync(cancellationToken);

            // Create enriched wishlist items
            var enrichedWishlist = new List<WishlistItemDto>();

            foreach (var wishlistItem in wishlistItems)
            {
                var ad = ads.FirstOrDefault(a => a.Id == wishlistItem.AdId);
                if (ad != null)
                {
                    enrichedWishlist.Add(new WishlistItemDto
                    {
                        AdId = ad.Id,
                        Title = ad.Title,
                        Description = ad.Description,
                        Price = ad.Price,
                        ImageUrl = ad.ImageUrl,
                        Location = ad.location,
                        CategoryName = ad.Category?.Title ?? "Unknown",
                        SubCategoryName = ad.SubCategory?.Title ?? "Unknown",
                        AddedAt = wishlistItem.AddedAt
                    });
                }
            }

            return new WishlistResponseDto
            {
                IsSuccess = true,
                Message = $"Found {enrichedWishlist.Count} items in your wishlist.",
                Items = enrichedWishlist,
                TotalCount = enrichedWishlist.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist for user {UserId}", userId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to retrieve wishlist."
            };
        }
    }

    public async Task<WishlistResponseDto> AddToWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            var wishlist = GetWishlistFromCache(cacheKey);

            // Check if item already exists in wishlist
            if (wishlist.Any(item => item.AdId == adId))
            {
                return new WishlistResponseDto
                {
                    IsSuccess = false,
                    Message = "Item is already in your wishlist.",
                    Items = wishlist,
                    TotalCount = wishlist.Count
                };
            }

            // Verify that the ad exists and is active
            var adExists = await _context.ads
                .AnyAsync(a => a.Id == adId && a.isActive, cancellationToken);

            if (!adExists)
            {
                return new WishlistResponseDto
                {
                    IsSuccess = false,
                    Message = "Ad not found or not active."
                };
            }

            // Add new item to wishlist
            var newItem = new WishlistItemDto
            {
                AdId = adId,
                AddedAt = DateTime.UtcNow
            };

            wishlist.Add(newItem);

            // Save back to cache
            _memoryCache.Set(cacheKey, wishlist, CacheDuration);

            return new WishlistResponseDto
            {
                IsSuccess = true,
                Message = "Item added to wishlist successfully.",
                Items = wishlist,
                TotalCount = wishlist.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist for user {UserId}", userId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to add item to wishlist."
            };
        }
    }

    public async Task<WishlistResponseDto> RemoveFromWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            var wishlist = GetWishlistFromCache(cacheKey);

            // Find and remove the item
            var itemToRemove = wishlist.FirstOrDefault(item => item.AdId == adId);
            if (itemToRemove == null)
            {
                return new WishlistResponseDto
                {
                    IsSuccess = false,
                    Message = "Item not found in wishlist.",
                    Items = wishlist,
                    TotalCount = wishlist.Count
                };
            }

            wishlist.Remove(itemToRemove);

            // Save back to cache
            _memoryCache.Set(cacheKey, wishlist, CacheDuration);

            return new WishlistResponseDto
            {
                IsSuccess = true,
                Message = "Item removed from wishlist successfully.",
                Items = wishlist,
                TotalCount = wishlist.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist for user {UserId}", userId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to remove item from wishlist."
            };
        }
    }

    public async Task<WishlistResponseDto> ClearWishlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            
            // Remove the entire wishlist from cache
            _memoryCache.Remove(cacheKey);

            return new WishlistResponseDto
            {
                IsSuccess = true,
                Message = "Wishlist cleared successfully.",
                Items = new List<WishlistItemDto>(),
                TotalCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for user {UserId}", userId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to clear wishlist."
            };
        }
    }

    public async Task<bool> IsInWishlistAsync(string userId, int adId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            var wishlist = GetWishlistFromCache(cacheKey);
            return wishlist.Any(item => item.AdId == adId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if item is in wishlist for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetWishlistCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"Wishlist_{userId}";
            var wishlist = GetWishlistFromCache(cacheKey);
            return wishlist.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist count for user {UserId}", userId);
            return 0;
        }
    }

    // IRepository<T> implementation methods
    public async Task<WishlistItemDto?> GetByIdAsync(int id)
    {
        // This method is not applicable for wishlist items as they don't have individual IDs
        // They are identified by AdId within a user's wishlist
        throw new NotImplementedException("GetByIdAsync is not applicable for wishlist items. Use GetWishlistAsync instead.");
    }

    public async Task<IEnumerable<WishlistItemDto>> GetAllAsync()
    {
        // This method is not applicable for wishlist items as they are user-specific
        throw new NotImplementedException("GetAllAsync is not applicable for wishlist items. Use GetWishlistAsync with userId instead.");
    }

    public async Task<WishlistItemDto> AddAsync(WishlistItemDto entity)
    {
        // This method is not applicable for wishlist items as they require userId context
        throw new NotImplementedException("AddAsync is not applicable for wishlist items. Use AddToWishlistAsync with userId instead.");
    }

    public async Task UpdateAsync(WishlistItemDto entity)
    {
        // This method is not applicable for wishlist items as they are immutable once added
        throw new NotImplementedException("UpdateAsync is not applicable for wishlist items.");
    }

    public async Task DeleteAsync(WishlistItemDto entity)
    {
        // This method is not applicable for wishlist items as they require userId context
        throw new NotImplementedException("DeleteAsync is not applicable for wishlist items. Use RemoveFromWishlistAsync with userId instead.");
    }

    public async Task<bool> ExistsAsync(int id)
    {
        // This method is not applicable for wishlist items as they don't have individual IDs
        throw new NotImplementedException("ExistsAsync is not applicable for wishlist items. Use IsInWishlistAsync with userId instead.");
    }

    private List<WishlistItemDto> GetWishlistFromCache(string cacheKey)
    {
        if (_memoryCache.TryGetValue(cacheKey, out List<WishlistItemDto>? wishlist))
        {
            return wishlist ?? new List<WishlistItemDto>();
        }

        return new List<WishlistItemDto>();
    }
} 