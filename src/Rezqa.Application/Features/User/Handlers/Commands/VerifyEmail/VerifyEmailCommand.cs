using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Handlers.Commands.VerifyEmail;

public record VerifyEmailCommand(VerifyEmailRequest Request) : IRequest<bool>;