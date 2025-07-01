using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly SignInManager<AppUsers> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IEmailService _emailService;

    public LoginCommandHandler(
        UserManager<AppUsers> userManager,
        SignInManager<AppUsers> signInManager,
        IOptions<JwtSettings> jwtSettings,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        if (user == null)
        {
            return new AuthResponseDto(
                IsSuccess: false,
                Message: "Invalid username or password."
            );
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            var Emailtoken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName!, Emailtoken);

            return new AuthResponseDto(
                IsSuccess: false,
                Message: "Email not confirmed. Verification email has been sent."
            );
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Request.Password, false);
        if (!result.Succeeded)
        {
            return new AuthResponseDto(
                IsSuccess: false,
                Message: "Invalid username or password."
            );
        }

        // Generate token and fetch roles
        var token = await GenerateAuthResponse(user); // Replace with your token logic
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto(
            IsSuccess: true,
            Message: "Login successful.",
            AccessToken: token,
            UserName: user.UserName!,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            ImageUrl: user.image
                    );
    }

    private async Task<string> GenerateAuthResponse(AppUsers user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret))
        {
            KeyId = "access-key-openSyria-stor"
        };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(1);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);

    }
}