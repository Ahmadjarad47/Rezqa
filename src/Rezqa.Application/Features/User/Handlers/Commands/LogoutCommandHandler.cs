using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly SignInManager<AppUsers> _signInManager;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        SignInManager<AppUsers> signInManager,
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