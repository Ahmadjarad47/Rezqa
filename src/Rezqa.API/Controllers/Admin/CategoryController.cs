using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Application.Features.Category.Requests.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rezqa.API.Controllers.Admin;

public class CategoryController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(IMediator mediator, ILogger<CategoryController> logger) : base(mediator)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetAll([FromQuery] GetAllCategoriesRequest request)
    {
        try
        {
            var categories = await _mediator.Send(request);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, $"An error occurred while retrieving categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        try
        {
            var category = await _mediator.Send(new GetCategoryByIdRequest { Id = id });
            return Ok(category);
        }
        catch (KeyNotFoundException ex) 
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, $"An error occurred while retrieving the category: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromForm] CreateCategoryRequest request)
    {
        try
        {
            var categoryId = await _mediator.Send(request);
            _logger.LogInformation("Category created with ID {CategoryId}", categoryId);
            return CreatedAtAction(nameof(GetById), new { id = categoryId }, categoryId);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, $"An error occurred while creating the category: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromForm] UpdateCategoryRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id mismatch");

        try
        {
            var success = await _mediator.Send(request);
            if (!success)
                return NotFound($"Category with ID {id} not found");

            _logger.LogInformation("Category {CategoryId} updated", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, $"An error occurred while updating the category: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteCategoryRequest { Id = id });
            if (!success)
                return NotFound($"Category with ID {id} not found");

            _logger.LogInformation("Category {CategoryId} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, $"An error occurred while deleting the category: {ex.Message}");
        }
    }

    /// <summary>
    /// Get active categories only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<PaginatedResult<CategoryDto>>> GetActiveCategories([FromQuery] GetAllCategoriesRequest request)
    {
        try
        {
            request.IsActive = true;
            var categories = await _mediator.Send(request);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active categories");
            return StatusCode(500, $"An error occurred while retrieving active categories: {ex.Message}");
        }
    }
}