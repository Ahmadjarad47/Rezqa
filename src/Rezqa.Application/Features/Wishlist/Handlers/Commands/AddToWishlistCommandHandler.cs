using MediatR;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Features.Wishlist.Request.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Wishlist.Handlers.Commands;

public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, WishlistResponseDto>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<AddToWishlistCommandHandler> _logger;

    public AddToWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        ILogger<AddToWishlistCommandHandler> logger)
    {
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _wishlistRepository.AddToWishlistAsync(request.UserId, request.Request.AdId, cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Added item {AdId} to wishlist for user {UserId}", 
                    request.Request.AdId, request.UserId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist for user {UserId}", request.UserId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to add item to wishlist."
            };
        }
    }
} 