using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Query;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.User.Handlers.Query;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly UserManager<AppUsers> _userManager;

    public GetAllUsersQueryHandler(UserManager<AppUsers> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            if (roles.Any(m => m.ToLower() != "Admin".ToLower()))
            {


                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    ImageUrl = user.image,
                    PhoneNumber = user.PhoneNumber,
                    isConfirmeEmail = user.EmailConfirmed,
                    isBlocked = user.LockoutEnd > DateTime.Now,
                    lockoutEnd = (DateTimeOffset)user.LockoutEnd,
                    Roles = roles.ToList()
                });
            }
        }

        return userDtos;
    }
}
