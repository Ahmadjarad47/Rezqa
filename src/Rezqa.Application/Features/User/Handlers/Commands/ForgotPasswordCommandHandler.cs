using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, AuthResponseDto>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        UserManager<AppUsers> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            // Return success even if user doesn't exist to prevent email enumeration
            return new AuthResponseDto(
                IsSuccess: true,
                Message: "If the email exists, a password reset link has been sent."
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetAsync(user.Email!, user.UserName!, token);

        return new AuthResponseDto(
            IsSuccess: true,
            Message: "Password reset email sent successfully."
        );
    }
}