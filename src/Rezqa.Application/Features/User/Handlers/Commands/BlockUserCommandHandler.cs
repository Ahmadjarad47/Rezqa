using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, bool>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly ILogger<BlockUserCommandHandler> _logger;

    public BlockUserCommandHandler(
        UserManager<AppUsers> userManager,
        ILogger<BlockUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Request.UserId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }
            if (user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var result1 = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(-10));

            }
            // Set the lockout end date
            var result = await _userManager.SetLockoutEndDateAsync(user, request.Request.BlockUntil);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserId} has been blocked until {BlockUntil}",
                    user.Id, request.Request.BlockUntil);
                return true;
            }

            _logger.LogError("Failed to block user {UserId}. Errors: {Errors}",
                user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {UserId}", request.Request.UserId);
            throw;
        }
    }
}