namespace Fleet.API.Controllers
{
    [ApiController]
    [Route("api/package")]

    public class PackageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PackageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePackage(CreatePackageCommand command, CancellationToken cancellationToken = default)
        {
            await _mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}

