using MediatR;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.Ad.Handlers.Commands
{
    public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommandRequest, bool>
    {
        private readonly IAdRepository adRepository;

        public UpdateStatusCommandHandler(IAdRepository adRepository)
        {
            this.adRepository = adRepository;
        }

        public async Task<bool> Handle(UpdateStatusCommandRequest request, CancellationToken cancellationToken)
        {

            var ad = await adRepository.GetByIdAsync(request.Id);
            if (!string.IsNullOrEmpty(request.userId) && ad.UserId.ToString() == request.userId)
            {

                ad.isActive = ad.isActive ? false : true;
            }
            else if (request.isAdmin)
            {
                ad.isActive = ad.isActive ? false : true;

            }
            else
            {
                return false;
            }
            await adRepository.UpdateAsync(ad);
            return true;
        }
    }
}
