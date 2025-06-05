using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Domain.Identity;
using Rezqa.Infrastructure.Services;

namespace Rezqa.Application.Features.User.Handlers.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        UserManager<IdentityUser> userManager,
        ILogger<RegisterCommandHandler> logger,
        IEmailService emailService)
    {
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            UserName = request.Request.UserName,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Request.Password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto(
                IsSuccess: false,
                Message: $"Failed to create user: {errorMessage}"
            );
        }

        // Assign default role
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
        }

        return new AuthResponseDto(
            IsSuccess: true,
            Message: "Registration successful. Verification email sent.",
            AccessToken: null, // No token on registration
            UserName: user.UserName,
            Email: user.Email,
            Roles: new List<string> { RoleSeeder.Roles.User }
        );
    }


}