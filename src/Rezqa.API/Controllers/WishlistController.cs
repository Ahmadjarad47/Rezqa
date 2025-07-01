using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Features.Wishlist.Dtos;
using Rezqa.Application.Features.Wishlist.Request.Commands;
using Rezqa.Application.Features.Wishlist.Request.Queries;
using Rezqa.Application.Interfaces;
using System.Security.Claims;

namespace Rezqa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(
        IMediator mediator,
        IWishlistRepository wishlistRepository,
        ILogger<WishlistController> logger)
    {
        _mediator = mediator;
        _wishlistRepository = wishlistRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get user's wishlist items
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wishlist items with full ad details</returns>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WishlistResponseDto>> GetWishlist(CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            WishlistResponseDto response = await _mediator.Send(new GetWishlistQuery(userId), cancellationToken);

            if (!response.IsSuccess)
            {
                return BadRequest(response.Items);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wishlist");
            return BadRequest("Failed to retrieve wishlist");
        }
    }

    /// <summary>
    /// Add an item to user's wishlist
    /// </summary>
    /// <param name="request">Ad ID to add to wishlist</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated wishlist</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WishlistResponseDto>> AddToWishlist(
        [FromBody] AddToWishlistDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var response = await _mediator.Send(new AddToWishlistCommand(userId, request), cancellationToken);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to wishlist");
            return BadRequest("Failed to add item to wishlist");
        }
    }

    /// <summary>
    /// Remove an item from user's wishlist
    /// </summary>
    /// <param name="request">Ad ID to remove from wishlist</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated wishlist</returns>
    [HttpPost("remove")]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WishlistResponseDto>> RemoveFromWishlist(
        [FromBody] RemoveFromWishlistDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var response = await _mediator.Send(new RemoveFromWishlistCommand(userId, request), cancellationToken);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from wishlist");
            return BadRequest("Failed to remove item from wishlist");
        }
    }

    /// <summary>
    /// Clear all items from user's wishlist
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Empty wishlist</returns>
    [HttpPost("clear")]
    [ProducesResponseType(typeof(WishlistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WishlistResponseDto>> ClearWishlist(CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var response = await _mediator.Send(new ClearWishlistCommand(userId), cancellationToken);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing wishlist");
            return BadRequest("Failed to clear wishlist");
        }
    }

    /// <summary>
    /// Check if an item is in user's wishlist
    /// </summary>
    /// <param name="adId">Ad ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boolean indicating if item is in wishlist</returns>
    [HttpGet("check/{adId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> IsInWishlist(int adId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var isInWishlist = await _wishlistRepository.IsInWishlistAsync(userId, adId, cancellationToken);

            return Ok(new { IsInWishlist = isInWishlist });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if item is in wishlist");
            return BadRequest("Failed to check wishlist");
        }
    }

    /// <summary>
    /// Get wishlist count for user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of items in wishlist</returns>
    [HttpGet("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> GetWishlistCount(CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var count = await _wishlistRepository.GetWishlistCountAsync(userId, cancellationToken);

            return Ok(new { Count = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wishlist count");
            return BadRequest("Failed to get wishlist count");
        }
    }
}