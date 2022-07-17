namespace Fleet.Core.Handlers.CommandHandlers;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand>
{
    private readonly FleetContext _context;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    public CreateVehicleCommandHandler(FleetContext context, ILogger<CreateVehicleCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateVehicleCommand request, CancellationToken cancellationToken = default)
    {

        bool result = await _context.Vehicles.Find(c => c.LicencePlate == request.LicencePlate).AnyAsync(cancellationToken);

        if (result)
        {
            _logger.LogWarning("Someone tried to insert existed deliverypoint. Request is {@request}", request);
            throw new VehicleExistException();
        }


        VehicleEntity entity = request.Adapt<VehicleEntity>();

        InsertOneOptions insertOneOptions = new();

        await _context.Vehicles.InsertOneAsync(entity, insertOneOptions, cancellationToken);

        //TODO: In feature , we need to fire an created event.
        _logger.LogInformation("Vehicle created. Request is {@request}", request);

        return Unit.Value;
    }
}

public record CreateVehicleCommand : IRequest
{
    public string LicencePlate { get; init; }
}

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(c => c.LicencePlate).NotEmpty();
    }
}

