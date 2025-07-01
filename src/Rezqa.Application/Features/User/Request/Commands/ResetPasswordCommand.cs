using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Request.Commands;

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<bool>;