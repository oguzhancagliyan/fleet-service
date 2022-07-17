namespace Fleet.Core.Handlers.CommandHandlers
{
    public class CreateDeliveryPointCommandHandler : IRequestHandler<CreateDeliveryPointCommand>
    {
        private readonly FleetContext _context;
        private readonly ILogger<CreateDeliveryPointCommandHandler> _logger;


        public CreateDeliveryPointCommandHandler(FleetContext context, ILogger<CreateDeliveryPointCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<Unit> Handle(CreateDeliveryPointCommand request, CancellationToken cancellationToken)
        {

            DeliveryPointEntity result = await _context.DeliveryPoints.Find(c => c.Name == request.Name && c.Value == request.Value).FirstOrDefaultAsync(cancellationToken);

            if (result != null)
            {
                _logger.LogWarning("Someone tried to insert existed deliverypoint. Request is {@request}", request);
                throw new DeliveryPointExistException();
            }

            var entity = request.Adapt<DeliveryPointEntity>();

            await _context.DeliveryPoints.InsertOneAsync(entity, null, cancellationToken);


            //TODO: In feature , we need to fire an created event.
            _logger.LogInformation("DeliveryPoint created. Request is {@request}", request);

            return Unit.Value;
        }
    }

    public record CreateDeliveryPointCommand : IRequest
    {
        public string Name { get; init; }

        public int Value { get; init; }

        public List<ShiptmenUnloadOption> UnloadOptions { get; init; }
    }

    public class CreateDeliveryPointCommandvalidator : AbstractValidator<CreateDeliveryPointCommand>
    {
        public CreateDeliveryPointCommandvalidator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Value).GreaterThan(0);
        }
    }
}

