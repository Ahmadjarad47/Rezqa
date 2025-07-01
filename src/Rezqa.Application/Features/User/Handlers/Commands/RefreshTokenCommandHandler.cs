using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Features.User.Settings;
using Rezqa.Domain.Entities;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rezqa.Application.Features.User.Handlers.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<AppUsers> userManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate the refresh token structure and get principal
        var principal = GetPrincipalFromExpiredToken(request.Request.RefreshToken);

        // 2. Check if token is expired by its claim
        var expiryClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

        if (string.IsNullOrWhiteSpace(expiryClaim))
        {
            throw new SecurityTokenException("Expiration claim is missing from the token.");
        }

        if (!long.TryParse(expiryClaim, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixExpiry))
        {
            throw new SecurityTokenException("Invalid expiration claim format.");
        }

        DateTime expiryUtc;
        try
        {
            expiryUtc = DateTimeOffset.FromUnixTimeSeconds(unixExpiry).UtcDateTime;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new SecurityTokenException("Invalid expiration timestamp in the token.");
        }

        if (expiryUtc < DateTime.UtcNow)
        {
            throw new SecurityTokenExpiredException("Refresh token has expired.");
        }
        // 3. Get user from the token
        var username = principal.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            throw new SecurityTokenException("Invalid token - no username claim");
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // 4. Generate new tokens
        var response = await GenerateAuthResponse(user, request.Request.Day);

        return response;
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false, // We manually validate expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            throw new SecurityTokenException("Invalid token");
        }
    }

    private async Task<AuthResponseDto> GenerateAuthResponse(AppUsers user, int day)
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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessTokenExpires = DateTime.UtcNow.AddDays(day);

        // Generate access token
        var accessToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: accessTokenExpires,
            signingCredentials: creds
        );

        return new AuthResponseDto(
            IsSuccess: true,
            Message: "Token refresh successful.",
            AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
            UserName: user.UserName!,
            Email: user.Email!,
            Roles: roles.ToList()
        );
    }
}