using MediatR;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Features.Wishlist.Request.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Wishlist.Handlers.Commands;

public class ClearWishlistCommandHandler : IRequestHandler<ClearWishlistCommand, WishlistResponseDto>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<ClearWishlistCommandHandler> _logger;

    public ClearWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        ILogger<ClearWishlistCommandHandler> logger)
    {
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> Handle(ClearWishlistCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _wishlistRepository.ClearWishlistAsync(request.UserId, cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Cleared wishlist for user {UserId}", request.UserId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist for user {UserId}", request.UserId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to clear wishlist."
            };
        }
    }
} 