using MediatR;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Features.Wishlist.Request.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Wishlist.Handlers.Queries;

public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistResponseDto>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<GetWishlistQueryHandler> _logger;

    public GetWishlistQueryHandler(
        IWishlistRepository wishlistRepository,
        ILogger<GetWishlistQueryHandler> logger)
    {
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    public async Task<WishlistResponseDto> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _wishlistRepository.GetWishlistAsync(request.UserId, cancellationToken);

            if (response.IsSuccess)
            {
                _logger.LogInformation("Retrieved wishlist for user {UserId} with {Count} items", 
                    request.UserId, response.TotalCount);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist for user {UserId}", request.UserId);
            return new WishlistResponseDto
            {
                IsSuccess = false,
                Message = "Failed to retrieve wishlist."
            };
        }
    }
} 