using MediatR;
using Rezqa.Application.Features.User.Dtos;

namespace Rezqa.Application.Features.User.Request.Commands;

public record GenerateTokenWithExpirationCommand(string UserId) : IRequest<string>;