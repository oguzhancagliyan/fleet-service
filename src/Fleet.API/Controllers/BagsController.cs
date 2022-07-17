using System;
namespace Fleet.API.Controllers;

[ApiController]
[Route("api/bag")]

public class BagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBag([FromBody] CreateBagCommand command, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpPost]
    [Route("assign")]
    public async Task<IActionResult> AssignPackages([FromBody] AssignPackagesCommand command, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);

        return Ok();
    }
}

