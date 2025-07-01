using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rezqa.Application.Features.User.Dtos;
using Rezqa.Application.Features.User.Request.Commands;
using Rezqa.Application.Features.User.Request.Query;

namespace Rezqa.API.Controllers.Admin
{
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger) : base(mediator)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<List<GetAllUsers>>> GetAll()
        {
            try
            {
                List<UserDto>? users = await _mediator.Send(new GetAllUsersQuery());
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }
        }

        [HttpPost("block-unblock")]
        public async Task<ActionResult<bool>> BlockUser([FromBody] BlockUserDto request)
        {
            try
            {
                var result = await _mediator.Send(new BlockUserCommand(request));
                _logger.LogInformation("User {UserId} block/unblock status changed", request.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking/unblocking user {UserId}", request.UserId);
                return StatusCode(500, $"An error occurred while updating user status: {ex.Message}");
            }
        }
    }
}
