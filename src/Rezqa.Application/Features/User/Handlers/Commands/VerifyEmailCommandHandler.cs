using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        UserManager<AppUsers> userManager,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Request.Token);
        return result.Succeeded;
    }
}