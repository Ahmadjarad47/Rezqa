using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class ResendEmailConfirmationCommandHandler : IRequestHandler<ResendEmailConfirmationCommand, AuthResponseDto>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly IEmailService _emailService;

    public ResendEmailConfirmationCommandHandler(
        UserManager<AppUsers> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            return new AuthResponseDto(
                IsSuccess: false,
                Message: "User not found."
            );
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponseDto(
                IsSuccess: false,
                Message: "Email is already confirmed."
            );
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName!, token);

        return new AuthResponseDto(
            IsSuccess: true,
            Message: "Email confirmation sent successfully."
        );
    }
}