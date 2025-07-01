using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Application.Features.SubCategory.Requests.Queries;

namespace Rezqa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdHomeController : ControllerBase
{
    private readonly IMediator mediator;

    public AdHomeController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpGet(template: "category")]
    public async Task<IActionResult> getAllCategory()
    {
        var result = await mediator.Send(new GetAllCategoriesRequest { isPagnationStop = true });
        return Ok(result.Items);
    }
    [HttpGet(template: "Subcategory")]
    public async Task<IActionResult> getSubCategory(int categoryId)
    {
        var result = await mediator.Send(new GetAllSubCategoriesRequest { isPagnationStop = true, CategoryId = categoryId });
        return Ok(result.Items);
    }

    [HttpGet(template: "ads")]
    public async Task<IActionResult> ads([FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
    {
        PaginatedResult<AdDto>? result = await mediator.Send(new GetAllAdsQuery
        {
            PageSize = pageSize,
            isfilterStop = true,
            PageNumber = pageNumber,
            search = search,

        });
        return Ok(result);
    }




    [HttpGet(template: "ads-id")]
    public async Task<IActionResult> adsId([FromQuery] int id,
          CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAdByIdQuery(id));
        if (result.IsSuccess)
        {

            return Ok(result.Data);
        }
        return BadRequest();
    }















    /// <summary>
    /// Search ads by term
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching ads</returns>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchAds(
        [FromQuery] string? searchTerm,
        [FromQuery] int? CategoryId,
        [FromQuery] int? SubCategoryId,
        [FromQuery] int PageNumber,
        [FromQuery] int PageSize,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? location,
        CancellationToken cancellationToken)
    {
        try
        {
            var ads = await mediator.Send(
                new SearchAdsQuery(searchTerm, CategoryId, SubCategoryId, PageSize, PageNumber, minPrice, maxPrice, location),
                cancellationToken);
            return Ok(ads);
        }
        catch (Exception ex)
        {
            // It's a good practice to log the exception
            return StatusCode(500, "An error occurred while searching ads");
        }
    }

}
