using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Handlers.Commands.ResendEmailConfirmation;

public record ResendEmailConfirmationCommand(ResendEmailConfirmationDto Request) : IRequest<bool>;