using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Features.Ad.Request.Query;

namespace Rezqa.API.Controllers.Admin
{
    public class AdsController : BaseController
    {
        private readonly IMediator mediator;

        public AdsController(IMediator mediator) : base(mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// Get all ads with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <param name="search">Optional search term</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of ads</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResult<AdDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedResult<AdDto>>> GetAllAds(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetAllAdsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    search = search
                };

                var result = await mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving ads");
            }
        }







        [HttpPut("update-Active-show")]
        public async Task<IActionResult> update(int id)
        {
            var result = await mediator.Send(new UpdateStatusCommandRequest() { Id = id });
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> delete([FromQuery] DeleteAdCommand deleteAdCommand)
        {
            var result = await mediator.Send(deleteAdCommand);
            return Ok(result);
        }
    }
}
