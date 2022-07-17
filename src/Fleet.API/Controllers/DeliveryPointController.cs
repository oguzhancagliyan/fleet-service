

namespace Fleet.API.Controllers;

[ApiController]
[Route("api/delivery_point")]
public class DeliveryPointController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryPointController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDeliveryPoint([FromBody]CreateDeliveryPointCommand command, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command, cancellationToken);

        return Ok();
    }


}

