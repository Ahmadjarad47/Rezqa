using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.Ad.Request.Commands
{
    public record UpdateStatusCommandRequest : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
