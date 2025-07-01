using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Features.Ad.Request.Query;
using System.Security.Claims;

namespace Rezqa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdController> _logger;

    public AdController(
        IMediator mediator,
        ILogger<AdController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }



    /// <summary>
    /// Get ad by ID
    /// </summary>
    /// <param name="id">Ad ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ad details</returns>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdResponseDto>> GetAdById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(new GetAdByIdQuery(id), cancellationToken);

            if (!response.IsSuccess)
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ad with ID {AdId}", id);
            return StatusCode(500, "An error occurred while retrieving the ad");
        }
    }

    /// <summary>
    /// Get ads by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of ads for the user</returns>
    [HttpGet("user")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AdDto>>> GetAdsByUserId(CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {

            if (!Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Invalid user identifier.");

            var ads = await _mediator.Send(new GetAdsByUserIdQuery(userId), cancellationToken);
            return Ok(ads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ads for user {UserId}", userIdString);
            return StatusCode(500, "An error occurred while retrieving user ads");
        }
    }

    /// <summary>
    /// Search ads by term
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching ads</returns>
    //[HttpGet("search")]
    //[AllowAnonymous]
    //[ProducesResponseType(typeof(List<AdDto>), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<ActionResult<List<AdDto>>> SearchAds(
    //    [FromQuery] string searchTerm,
    //    CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(searchTerm))
    //            return BadRequest("Search term is required");

    //        var ads = await _mediator.Send(new SearchAdsQuery(searchTerm), cancellationToken);
    //        return Ok(ads);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to search ads with term {SearchTerm}", searchTerm);
    //        return StatusCode(500, "An error occurred while searching ads");
    //    }
    //}

    /// <summary>
    /// Create a new ad
    /// </summary>
    /// <param name="request">Ad creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created ad details</returns>
    [HttpPost("create-ad")]
    [ProducesResponseType(typeof(AdResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdResponseDto>> CreateAd(
        [FromForm] CreateAdDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }
            request.UserId = userId;
            // تحقق من نوع وحجم الملفات (مثال)
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 5 * 1024 * 1024) // 5MB
                        return BadRequest("File size must be less than 5MB");
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(file.ContentType))
                        return BadRequest("Only image files are allowed");
                }
            }
            var response = await _mediator.Send(new CreateAdCommand(request), cancellationToken);
            if (!response.IsSuccess)
                return BadRequest(response);
            return CreatedAtAction(nameof(GetAdById), new { id = response.Data?.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ad for user {UserId}", request.UserId);
            return StatusCode(500, "Unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Update an existing ad
    /// </summary>
    /// <param name="id">Ad ID</param>
    /// <param name="request">Ad update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated ad details</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdResponseDto>> UpdateAd(
        int id,
        [FromForm] UpdateAdDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.Id = id;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }
            // تحقق من ملكية المستخدم للإعلان قبل التعديل
            var ad = await _mediator.Send(new GetAdByIdQuery(id), cancellationToken);
            if (ad?.Data == null)
                return NotFound("Ad not found");
            if (ad.Data.UserId != userId)
                return Forbid("You are not allowed to update this ad");
            // تحقق من نوع وحجم الملفات (مثال)
            if (Request.Form.Files.Count > 0)
            {
                foreach (var file in Request.Form.Files)
                {
                    if (file.Length > 5 * 1024 * 1024) // 5MB
                        return BadRequest("File size must be less than 5MB");
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(file.ContentType))
                        return BadRequest("Only image files are allowed");
                }
            }
            var response = await _mediator.Send(new UpdateAdCommand(request), cancellationToken);
            if (!response.IsSuccess)
            {
                if (response.Message.Contains("not found"))
                    return NotFound(response);
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update ad {AdId} for user", id);
            return StatusCode(500, "Unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Delete an ad
    /// </summary>
    /// <param name="id">Ad ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(AdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AdResponseDto>> DeleteAd(int id, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }
            // تحقق من ملكية المستخدم للإعلان قبل الحذف
            var ad = await _mediator.Send(new GetAdByIdQuery(id), cancellationToken);
            if (ad?.Data == null)
                return NotFound("Ad not found");
            if (ad.Data.UserId != userId)
                return Forbid("You are not allowed to delete this ad");
            var response = await _mediator.Send(new DeleteAdCommand(id, userIdClaim), cancellationToken);
            if (!response.IsSuccess)
            {
                if (response.Message.Contains("not found"))
                    return NotFound(response);
                return BadRequest(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete ad {AdId}", id);
            return StatusCode(500, "Unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Get current user's ads
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of current user's ads</returns>
    [HttpGet("my-ads")]
    [ProducesResponseType(typeof(List<AdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AdDto>>> GetMyAds(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var ads = await _mediator.Send(new GetAdsByUserIdQuery(userId), cancellationToken);
            return Ok(ads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve current user's ads");
            return StatusCode(500, "An error occurred while retrieving your ads");
        }
    }
}