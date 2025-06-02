using MediatR;

namespace Rezqa.Application.Features.User.Handlers.Commands.Logout;

public record LogoutCommand : IRequest<bool>; 