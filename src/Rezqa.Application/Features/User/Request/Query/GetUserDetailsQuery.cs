using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Request.Query;

public record GetUserDetailsQuery(string UserId) : IRequest<UserDetailsDto>; 