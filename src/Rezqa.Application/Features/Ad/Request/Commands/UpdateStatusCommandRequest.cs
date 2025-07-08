using MediatR;
namespace Rezqa.Application.Features.Ad.Request.Commands
{
    public record UpdateStatusCommandRequest : IRequest<bool>
    {
        public string? userId { get; set; } = string.Empty;
        public int Id { get; set; }
        public bool isAdmin { get; set; } = false;
    }
}
