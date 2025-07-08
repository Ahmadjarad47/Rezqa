using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Features.User.Request.Query;
using System.Security.Claims;
using System.Threading;

namespace Rezqa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;


    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger
)
    {
        _mediator = mediator;
        _logger = logger;

    }

    [HttpGet("is-auth")]
    public async Task<ActionResult<bool>> IsAuthenticated()
    {
        return User.Identity.IsAuthenticated ? Ok() : Unauthorized();
    }
    [HttpGet("user-data")]
    [Authorize]
    public async Task<IActionResult> GetProfileData()
    {
        var userData = await _mediator.Send(new GetUserDataQuery(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        return Ok(userData);
    }

    /// <summary>
    /// Get comprehensive user details including statistics
    /// </summary>
    /// <returns>Detailed user information with ads statistics</returns>
    [HttpGet("user-details")]
    [Authorize]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDetailsDto>> GetUserDetails()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            var userDetails = await _mediator.Send(new GetUserDetailsQuery(userId));
            return Ok(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user details");
            return StatusCode(500, "An error occurred while retrieving user details");
        }
    }

    /// <summary>
    /// Get comprehensive user details by user ID (Admin only)
    /// </summary>
    /// <param name="userId">The ID of the user to get details for</param>
    /// <returns>Detailed user information with ads statistics</returns>
    [HttpGet("user-details/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDetailsDto>> GetUserDetailsById(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }

            var userDetails = await _mediator.Send(new GetUserDetailsQuery(userId));
            return Ok(userDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user details for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user details");
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromForm] RegisterCommandRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(new RegisterCommand(request), cancellationToken);
            if (!response.IsSuccess)
                return BadRequest(response);

            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Registration failed for user {UserName}", request.UserName);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Login user and get JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginCommandRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto? response = await _mediator.Send(new LoginCommand(request), cancellationToken);
            if (!response.IsSuccess)
                return BadRequest(response);

            // Set access token cookie
            SetAccessTokenCookie(response.AccessToken!);

            var refresh = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(response.AccessToken!, 5)));
            // Set refresh token cookie (in a real app, generate a separate refresh token)
            SetRefreshTokenCookie(refresh.AccessToken!);

            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", request.Email);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Login user and get JWT token (Flutter compatible)
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token in response body</returns>
    [HttpPost("login-flutter")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> LoginFlutter(
        [FromBody] LoginCommandRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto? response = await _mediator.Send(new LoginCommand(request), cancellationToken);
            if (!response.IsSuccess)
                return BadRequest(response);

            // Set access token cookie
            //SetAccessTokenCookie(response.AccessToken!);

            var refresh = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(response.AccessToken!, 5)));
            // Set refresh token cookie
            //SetRefreshTokenCookie(refresh.AccessToken!);

            // إرجاع التوكن في الـ response body للـ Flutter
            return Ok(new
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                AccessToken = response.AccessToken,
                RefreshToken = refresh.AccessToken,
                UserName = response.UserName,
                Email = response.Email,
                PhoneNumber = response.PhoneNumber,
                ImageUrl = response.ImageUrl,
                Roles = response.Roles
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Login failed for user {Email}", request.Email);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication response with JWT token</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            var response = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(refreshToken, 1)),
                cancellationToken);

            var refresh = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(refreshToken, 5)));

            // Set new refresh token cookie
            SetAccessTokenCookie(response.AccessToken!);

            SetRefreshTokenCookie(refresh.AccessToken!);

            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Verify user's email
    /// </summary>
    /// <param name="request">Verification details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new VerifyEmailCommand(request), cancellationToken);
            return Ok(new { Success = result });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Email verification failed for {Email}", request.Email);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reset user's password
    /// </summary>
    /// <param name="request">Reset password details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new ResetPasswordCommand(request), cancellationToken);
            return Ok(new { Success = result });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Password reset failed for {Email}", request.Email);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Logout user and generate new token with expiration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status with new token</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new LogoutCommand(), cancellationToken);

            // Get current user from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            // Generate a new token with very short expiration (1 minute) to invalidate the session
            try
            {
                var response = await _mediator.Send(
                    new GenerateTokenWithExpirationCommand(userId), // 1 minute expiration
                    cancellationToken);

                SetAccessTokenCookie(response);
                SetRefreshTokenCookie(response);

                return Ok(new
                {
                    Success = true,
                    Message = "Logout successful. New token generated with short expiration.",
                    Token = response,
                    ExpiresIn = "1 minute",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss UTC")
                });

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate new token during logout");
            }

            // Fallback: Clear cookies if token generation fails
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-30),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
            };

            Response.Cookies.Append("accessToken", string.Empty, cookieOptions);
            Response.Cookies.Append("refreshToken", string.Empty, cookieOptions);

            return Ok(new { Success = true, Message = "Logout successful. Cookies cleared." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return BadRequest(ex.Message);
        }
    }
    /// <summary>
    /// Resends email confirmation link
    /// </summary>
    /// <param name="dto">Email address to resend confirmation to</param>
    /// <returns>Success message</returns>
    [HttpPost("resend-confirmation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto dto)
    {

        var command = new ResendEmailConfirmationCommand(dto);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        // Always return success to prevent email enumeration
        return Ok(new { message = "If your email is not confirmed, you will receive a new confirmation email." });
    }

    /// <summary>
    /// Initiates the password reset process by sending a reset link to the user's email
    /// </summary>
    /// <param name="request">Email address to send reset link to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ForgotPasswordCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            // Always return success to prevent email enumeration
            return result.IsSuccess ? Ok(new { message = "سوف تتلقى رابط إعادة تعيين كلمة المرور." })
                : BadRequest(new { message = "هذا البريد الإلكتروني غير مسجل" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password request failed for {Email}", request.Email);
            return BadRequest("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Refresh JWT token using refresh token (Flutter compatible)
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication response with JWT token in response body</returns>
    [HttpPost("refresh-token-flutter")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> RefreshTokenFlutter(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest("Refresh token is required");
            }

            var response = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(request.RefreshToken, 1)),
                cancellationToken);

            var refresh = await _mediator.Send(
                new RefreshTokenCommand(new RefreshTokenRequest(request.RefreshToken, 5)));

            // Set new refresh token cookie
            SetAccessTokenCookie(response.AccessToken!);
            SetRefreshTokenCookie(refresh.AccessToken!);

            // إرجاع التوكن في الـ response body للـ Flutter
            return Ok(new
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                AccessToken = response.AccessToken,
                RefreshToken = refresh.AccessToken,
                UserName = response.UserName,
                Email = response.Email,
                PhoneNumber = response.PhoneNumber,
                ImageUrl = response.ImageUrl,
                Roles = response.Roles
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Test endpoint to verify token reception method
    /// </summary>
    /// <returns>Information about how the token was received</returns>
    [HttpGet("test-token-reception")]
    [Authorize]
    public ActionResult TestTokenReception()
    {
        var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
        var accessTokenCookie = Request.Cookies["accessToken"];

        var tokenSource = "Unknown";
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            tokenSource = "Authorization Header (Flutter)";
        }
        else if (!string.IsNullOrEmpty(accessTokenCookie))
        {
            tokenSource = "HttpOnly Cookie (Angular)";
        }

        return Ok(new
        {
            Message = "Token received successfully",
            TokenSource = tokenSource,
            HasAuthorizationHeader = !string.IsNullOrEmpty(authorizationHeader),
            HasAccessTokenCookie = !string.IsNullOrEmpty(accessTokenCookie),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = User.FindFirstValue(ClaimTypes.Name),
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        });
    }

    private void SetAccessTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            //Domain = "https://syrianstore.runasp.net/",
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };

        Response.Cookies.Append("accessToken", token, cookieOptions);

        // إضافة التوكن في الـ response body أيضاً للعمل مع Flutter
        Response.Headers.Add("X-Access-Token", token);
    }

    private void SetRefreshTokenCookie(string token)
    {

        var cookieOptions = new CookieOptions
        {
            //Domain = "https://syrianstore.runasp.net/",
            Expires = DateTime.Now.AddDays(5),
            HttpOnly = true,

            IsEssential = true,
            Secure = true,
            Path = "/",
            SameSite = SameSiteMode.None,
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);

        // إضافة التوكن في الـ response body أيضاً للعمل مع Flutter
        Response.Headers.Add("X-Refresh-Token", token);
    }
}