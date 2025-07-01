using MediatR;
using Rezqa.Application.Features.DynamicField.Requests.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.DynamicField.Handlers.Commands.Delete;

public class DeleteDynamicFieldCommand : IRequestHandler<DeleteDynamicFieldRequest, bool>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public DeleteDynamicFieldCommand(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<bool> Handle(DeleteDynamicFieldRequest request, CancellationToken cancellationToken)
    {
        var dynamicField = await _dynamicFieldRepository.GetByIdAsync(request.Id);
        if (dynamicField == null)
        {
            return false;
        }

        await _dynamicFieldRepository.DeleteAsync(dynamicField);
        return true;
    }
} 