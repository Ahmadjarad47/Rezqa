using MediatR;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Features.Wishlist.Request.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Wishlist.Handlers.Commands;

public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, WishlistResponseDto>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<RemoveFromWishlistCommandHandler> _logger;

    public RemoveFromWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        ILogger<RemoveFromWishlistCommandHandler> logger)
    {
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _wishlistRepository.RemoveFromWishlistAsync(request.UserId, request.Request.AdId, cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Removed item {AdId} from wishlist for user {UserId}", 
                    request.Request.AdId, request.UserId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist for user {UserId}", request.UserId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to remove item from wishlist."
            };
        }
    }
} 