using MediatR;
using Rezqa.Application.Features.User.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.User.Request.Query
{
    public record GetUserDataQuery(string userId) : IRequest<UserDto>;

}
