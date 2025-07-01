using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Query;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Query;

public class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, UserDetailsDto>
{
    private readonly UserManager<AppUsers> _userManager;
    private readonly IAdRepository _adRepository;

    public GetUserDetailsQueryHandler(
        UserManager<AppUsers> userManager,
        IAdRepository adRepository)
    {
        _userManager = userManager;
        _adRepository = adRepository;
    }

    public async Task<UserDetailsDto> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new UserDetailsDto();
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Get user ads statistics
        var userAds = await _adRepository.GetAdsByUserIdAsync(user.Id);
        var totalAds = userAds.Count();
        var activeAds = userAds.Count(ad => ad.isActive);
        var inactiveAds = totalAds - activeAds;

        return new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ImageUrl = user.image,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnabled = user.LockoutEnabled,
            IsBlocked = user.LockoutEnd > DateTime.Now,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,

            Roles = roles.ToList(),
            TotalAds = totalAds,
            ActiveAds = activeAds,
            InactiveAds = inactiveAds
        };
    }
}