using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rezqa.API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class BaseController : ControllerBase
    {
        protected readonly IMediator mediator;

        public BaseController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        [HttpGet("is-auth")]
        public async Task<ActionResult<bool>> IsAuthenticated()
        {
            return User.Identity.IsAuthenticated ? Ok() : Unauthorized();
        }

    }
}
