using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Commands;

public class DeleteDynamicFieldRequest : IRequest<bool>
{
    [Required]
    public int Id { get; set; }
} 