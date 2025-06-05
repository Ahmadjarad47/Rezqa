using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Handlers.Commands.Login;
using Rezqa.Application.Features.User.Handlers.Commands.Logout;
using Rezqa.Application.Features.User.Handlers.Commands.RefreshToken;
using Rezqa.Application.Features.User.Handlers.Commands.Register;
using Rezqa.Application.Features.User.Handlers.Commands.ResetPassword;
using Rezqa.Application.Features.User.Handlers.Commands.VerifyEmail;
using Rezqa.Application.Features.User.Handlers.Commands.ResendEmailConfirmation;
using Rezqa.Application.Features.User.Handlers.Commands.ForgotPassword;
namespace Rezqa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ResponseCache(CacheProfileName = "Default30")]
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

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterCommandRequestDTO request,
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
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginCommandRequestDTO request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(new LoginCommand(request), cancellationToken);
            if (!response.IsSuccess)
                return BadRequest(response);

            // Set access token cookie
            SetAccessTokenCookie(response.AccessToken!);

            // Set refresh token cookie (in a real app, generate a separate refresh token)
            SetRefreshTokenCookie(response.AccessToken!);

            return Ok(response);
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
    [ResponseCache(NoStore = true)]
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
                new RefreshTokenCommand(new RefreshTokenRequest(refreshToken)),
                cancellationToken);

            // Set new refresh token cookie
            SetRefreshTokenCookie(response.AccessToken!); // In real app, generate a new refresh token

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
    [ResponseCache(NoStore = true)]
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
    [ResponseCache(NoStore = true)]
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
    /// Logout user and clear cookies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new LogoutCommand(), cancellationToken);

            // Clear both cookies
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { Success = true });
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

        if (!result)
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
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ForgotPasswordCommand(request);
            var result = await _mediator.Send(command, cancellationToken);

            // Always return success to prevent email enumeration
            return result ? Ok(new { message = "سوف تتلقى رابط إعادة تعيين كلمة المرور." })
                : BadRequest(new { message = "هذا البريد الإلكتروني غير مسجل" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password request failed for {Email}", request.Email);
            return BadRequest("An error occurred while processing your request.");
        }
    }

    private void SetAccessTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15) // Match JWT expiration
        };

        Response.Cookies.Append("accessToken", token, cookieOptions);
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7) // Longer expiration for refresh token
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}