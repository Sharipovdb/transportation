using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Transportation.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public abstract class BaseController : ControllerBase
{
    public readonly IMediator _mediator;

    public BaseController(IMediator mediator)
    {
        _mediator = mediator;
    }
}