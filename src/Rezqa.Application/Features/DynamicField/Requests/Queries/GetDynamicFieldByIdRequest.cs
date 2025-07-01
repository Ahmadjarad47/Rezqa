using MediatR;
using Rezqa.Application.Features.DynamicField.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Queries;

public class GetDynamicFieldByIdRequest : IRequest<DynamicFieldDto?>
{
    [Required]
    public int Id { get; set; }
} 