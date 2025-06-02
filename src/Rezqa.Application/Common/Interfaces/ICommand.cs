namespace Rezqa.Application.Common.Interfaces;

public interface ICommand<TResponse> : MediatR.IRequest<TResponse>
{
} 