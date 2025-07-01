using MediatR;

namespace Rezqa.Application.Features.User.Request.Commands;

public record LogoutCommand : IRequest<bool>; 