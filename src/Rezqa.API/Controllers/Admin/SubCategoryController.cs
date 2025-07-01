using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.SubCategory.Dtos;
using Rezqa.Application.Features.SubCategory.Requests.Commands;
using Rezqa.Application.Features.SubCategory.Requests.Queries;

namespace Rezqa.API.Controllers.Admin;

public class SubCategoryController : BaseController
{
    private readonly IMediator _mediator;   
    private readonly ILogger<SubCategoryController> _logger;

    public SubCategoryController(IMediator mediator, ILogger<SubCategoryController> logger) : base(mediator)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all sub-categories with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<SubCategoryDto>>> GetAll([FromQuery] GetAllSubCategoriesRequest request)
    {
        try
        {
            var subCategories = await _mediator.Send(request);
            return Ok(subCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategories");
            return StatusCode(500, $"An error occurred while retrieving sub-categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Get sub-category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SubCategoryDto>> GetById(int id)
    {
        try
        {
            var subCategory = await _mediator.Send(new GetSubCategoryByIdRequest { Id = id });
            return Ok(subCategory);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategory {SubCategoryId}", id);
            return StatusCode(500, $"An error occurred while retrieving the sub-category: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new sub-category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateSubCategoryRequest request)
    {
        try
        {
            var subCategoryId = await _mediator.Send(request);
            _logger.LogInformation("SubCategory created with ID {SubCategoryId}", subCategoryId);
            return CreatedAtAction(nameof(GetById), new { id = subCategoryId }, subCategoryId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subcategory");
            return StatusCode(500, $"An error occurred while creating the sub-category: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing sub-category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateSubCategoryRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id mismatch");

        try
        {
            var success = await _mediator.Send(request);
            if (!success)
                return NotFound($"SubCategory with ID {id} not found");

            _logger.LogInformation("SubCategory {SubCategoryId} updated", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subcategory {SubCategoryId}", id);
            return StatusCode(500, $"An error occurred while updating the sub-category: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a sub-category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteSubCategoryRequest { Id = id });
            if (!success)
                return NotFound($"SubCategory with ID {id} not found");

            _logger.LogInformation("SubCategory {SubCategoryId} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subcategory {SubCategoryId}", id);
            return StatusCode(500, $"An error occurred while deleting the sub-category: {ex.Message}");
        }
    }

    /// <summary>
    /// Get sub-categories by category ID
    /// </summary>
    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<PaginatedResult<SubCategoryDto>>> GetByCategory(int categoryId, [FromQuery] GetAllSubCategoriesRequest request)
    {
        try
        {
            request.CategoryId = categoryId;
            var subCategories = await _mediator.Send(request);
            return Ok(subCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategories for category {CategoryId}", categoryId);
            return StatusCode(500, $"An error occurred while retrieving sub-categories for category {categoryId}: {ex.Message}");
        }
    }
}