using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Commands;
using Rezqa.Application.Features.DynamicField.Requests.Queries;

namespace Rezqa.API.Controllers.Admin;

public class DynamicFieldController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<DynamicFieldController> _logger;

    public DynamicFieldController(IMediator mediator, ILogger<DynamicFieldController> logger) : base(mediator)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<DynamicFieldDto>>> GetAll([FromQuery] GetAllDynamicFieldsRequest request)
    {
        try
        {
            var dynamicFields = await _mediator.Send(request);
            return Ok(dynamicFields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dynamic fields");
            return StatusCode(500, $"An error occurred while retrieving dynamic fields: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DynamicFieldDto>> GetById(int id)
    {
        try
        {
            var dynamicField = await _mediator.Send(new GetDynamicFieldByIdRequest { Id = id });
            if (dynamicField == null)
                return NotFound($"Dynamic field with ID {id} not found");

            return Ok(dynamicField);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dynamic field {DynamicFieldId}", id);
            return StatusCode(500, $"An error occurred while retrieving the dynamic field: {ex.Message}");
        }
    }


    [HttpPost]
    public async Task<ActionResult<List<int>>> CreateBulk([FromBody] CreateDynamicFieldsRequest request)
    {
        try
        {
            var dynamicFieldIds = await _mediator.Send(request);
            _logger.LogInformation("Created {Count} dynamic fields", dynamicFieldIds.Count);
            return CreatedAtAction(nameof(GetAll), null, dynamicFieldIds);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dynamic fields in bulk");
            return StatusCode(500, $"An error occurred while creating the dynamic fields: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDynamicFieldRequest request)
    {
        if (id != request.Id)
            return BadRequest("Id mismatch");

        try
        {
            var success = await _mediator.Send(request);
            if (!success)
                return NotFound($"Dynamic field with ID {id} not found");

            _logger.LogInformation("Dynamic field {DynamicFieldId} updated", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dynamic field {DynamicFieldId}", id);
            return StatusCode(500, $"An error occurred while updating the dynamic field: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteDynamicFieldRequest { Id = id });
            if (!success)
                return NotFound($"Dynamic field with ID {id} not found");

            _logger.LogInformation("Dynamic field {DynamicFieldId} deleted", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dynamic field {DynamicFieldId}", id);
            return StatusCode(500, $"An error occurred while deleting the dynamic field: {ex.Message}");
        }
    }
}