using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.User.Handlers.Commands.Logout;

namespace Rezqa.Application.Features.User.Handlers.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        SignInManager<IdentityUser> signInManager,
        ILogger<LogoutCommandHandler> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _signInManager.SignOutAsync();
        return true;
    }
} 