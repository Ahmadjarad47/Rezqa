using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Handlers.Commands.Register;

public record RegisterCommand(RegisterCommandRequestDTO Request) : IRequest<AuthResponseDto>;