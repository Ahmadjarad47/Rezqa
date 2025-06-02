using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Infrastructure.Services;

namespace Rezqa.Application.Features.User.Handlers.Commands.ResendEmailConfirmation;

public class ResendEmailConfirmationCommandHandler : IRequestHandler<ResendEmailConfirmationCommand, bool>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendEmailConfirmationCommandHandler> _logger;

    public ResendEmailConfirmationCommandHandler(
        UserManager<IdentityUser> userManager,
        IEmailService emailService,
        ILogger<ResendEmailConfirmationCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Request.Email);
            if (user == null)
            {
                // Return success even if user doesn't exist to prevent email enumeration
                return true;
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return true;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName!, token);

            _logger.LogInformation("Email confirmation resent for user {UserId}", user.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation for {Email}", request.Request.Email);
            return false;
        }
    }
}