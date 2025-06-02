using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Handlers.Commands.Login;

public record LoginCommand(LoginCommandRequestDTO Request) : IRequest<AuthResponseDto>;