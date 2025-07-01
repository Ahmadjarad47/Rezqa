using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<AppUsers> userManager,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);

        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Request.Token, request.Request.NewPassword);
        return result.Succeeded;
    }
}