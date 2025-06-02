using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Domain.Identity;
using Rezqa.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Rezqa.Domain.Identity.RoleSeeder;
namespace Rezqa.Application.Features.User.Handlers.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<RegisterCommandHandler> logger,
        IEmailService emailService)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Request.UserName,
            Email = request.Request.Email,
            PhoneNumber = request.Request.Phone,
            EmailConfirmed = false // Ensure email is not confirmed initially
        };

        var result = await _userManager.CreateAsync(user, request.Request.Password);
        if (!result.Succeeded)
        {
            throw new ApplicationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign User role by default
        await _userManager.AddToRoleAsync(user, RoleSeeder.Roles.User);

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Send verification email
        try
        {
            await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName!, token);
            _logger.LogInformation("Verification email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            // Don't throw here, as the user is already created
            // The user can request a new verification email later
        }

        return new AuthResponseDto(
            AccessToken: string.Empty,
            UserName: user.UserName!,
            Email: user.Email!,
            Roles: []
        );
    }

}