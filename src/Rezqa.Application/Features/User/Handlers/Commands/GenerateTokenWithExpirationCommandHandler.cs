using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class GenerateTokenWithExpirationCommandHandler : IRequestHandler<GenerateTokenWithExpirationCommand, string>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<GenerateTokenWithExpirationCommandHandler> _logger;

    public GenerateTokenWithExpirationCommandHandler(
        UserManager<AppUsers> userManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<GenerateTokenWithExpirationCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<string> Handle(GenerateTokenWithExpirationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);


        return await GenerateAuthResponse(user);
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
        var expires = DateTime.UtcNow.AddDays(-10);

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