using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Handlers.Commands.ForgotPassword;

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<bool>; 