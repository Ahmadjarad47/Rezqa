using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Commands;

public class CreateDynamicFieldsRequest : IRequest<List<int>>
{
    [Required]
    public List<CreateDynamicFieldRequest> DynamicFields { get; set; } = new List<CreateDynamicFieldRequest>();
} 