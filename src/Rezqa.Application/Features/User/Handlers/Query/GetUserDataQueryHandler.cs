using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Query;
using Rezqa.Domain.Entities;


namespace Rezqa.Application.Features.User.Handlers.Query
{
    internal class GetUserDataQueryHandler : IRequestHandler<GetUserDataQuery, UserDto>
    {
        private readonly UserManager<AppUsers> userManager;

        public GetUserDataQueryHandler(UserManager<AppUsers> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<UserDto> Handle(GetUserDataQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.userId);
            if (user is null)
            {
                return new UserDto();
            }
            return new UserDto
            {
                Email = user.Email,
                ImageUrl = user.image,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
            };
        }
    }
}
