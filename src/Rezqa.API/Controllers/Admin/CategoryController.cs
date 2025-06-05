using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Application.Features.Category.Requests.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rezqa.API.Controllers.Admin;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _mediator.Send(new GetAllCategoriesRequest());
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
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
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromForm] CreateCategoryRequest request)
    {
        request.CreatedBy = User.Identity.Name;
        var categoryId = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = categoryId }, categoryId);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromForm] UpdateCategoryRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id mismatch");

        var success = await _mediator.Send(request);
        if (!success)
            return NotFound($"Category with ID {id} not found");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteCategoryRequest { Id = id });
        if (!success)
            return NotFound($"Category with ID {id} not found");

        return NoContent();
    }
} 