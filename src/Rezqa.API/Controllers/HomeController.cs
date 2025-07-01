using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;

namespace Rezqa.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly IMediator mediator;

    public HomeController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetAll([FromQuery] GetAllCategoriesRequest request)
    {
        try
        {
            var categories = await mediator.Send(request);
            return Ok(categories);
        }
        catch (Exception ex)
        {

            return StatusCode(500, $"An error occurred while retrieving categories: {ex.Message}");
        }
    }
}
