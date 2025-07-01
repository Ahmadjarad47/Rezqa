using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Queries;
using Rezqa.Application.Features.SubCategory.Dtos;
using Rezqa.Application.Features.SubCategory.Requests.Queries;

namespace Rezqa.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetAdListsController : ControllerBase
    {
        private readonly IMediator mediator;

        public GetAdListsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("get-categories")]
        public async Task<ActionResult> GetAllCategory([FromQuery] GetAllCategoriesRequest request)
        {
            PaginatedResult<CategoryDto>?
                result = await mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("get-subcategories")]
        public async Task<ActionResult> GetAllSubCategory([FromQuery] GetAllSubCategoriesRequest request)
        {
            try
            {
                PaginatedResult<SubCategoryDto>?
                result = await mediator.Send(request);
                return Ok(result);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An error occurred while retrieving dynamic fields: {ex.Message}");
            }
        }

        [HttpGet("get-dynamic-field")]
        public async Task<ActionResult<List<DynamicFieldDto>>> GetAll([FromQuery] GetDynamicFieldsForAdsRequest request)
        {
            try
            {
                var dynamicFields = await mediator.Send(request);
                return Ok(dynamicFields);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"An error occurred while retrieving dynamic fields: {ex.Message}");
            }
        }
    }
}
