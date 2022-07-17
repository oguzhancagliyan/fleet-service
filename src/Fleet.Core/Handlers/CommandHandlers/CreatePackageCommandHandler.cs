using System;

namespace Fleet.Core.Handlers.CommandHandlers
{
    public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand>
    {
        private readonly FleetContext _context;
        private readonly ILogger<CreateBagCommandHandler> _logger;

        public CreatePackageCommandHandler(FleetContext context, ILogger<CreateBagCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
        {
            List<int> deliveryPointList = request.Items.GroupBy(c => c.DeliveryPoint).Select(cd => cd.Key).ToList();
            var deliveryPointFilter = Builders<DeliveryPointEntity>.Filter.In(c => c.Value, deliveryPointList);

            long deliveryPointCount = await _context.DeliveryPoints.Find(deliveryPointFilter).CountDocumentsAsync(cancellationToken: cancellationToken);

            if (deliveryPointCount != deliveryPointList.Count())
            {
                _logger.LogError("Someone tried to create a bad with nonexisted delivery point. Request : {@request}", request);

                throw new DeliveryPointNotExistException();
            }

            var entityList = request.Items.Adapt<List<PackageEntity>>();

            await _context.Packages.InsertManyAsync(entityList, null, cancellationToken);

            return Unit.Value;
        }
    }

    public record CreatePackageCommand : IRequest
    {
        public List<PackageItem> Items { get; init; }
    }

    public class CreatePackageCommandValidator : AbstractValidator<CreatePackageCommand>
    {
        public CreatePackageCommandValidator()
        {
            RuleFor(c => c.Items).NotEmpty();
            RuleForEach(c => c.Items).SetValidator(new PackageItemValidator());
        }
    }

    public class PackageItemValidator : AbstractValidator<PackageItem>
    {
        public PackageItemValidator()
        {
            RuleFor(c => c.Barcode).NotEmpty();
            RuleFor(c => c.DeliveryPoint).GreaterThan(0);
            RuleFor(c => c.VolumetricWeight).GreaterThan(0);
        }
    }

    public record PackageItem
    {
        public string Barcode { get; set; }

        public int DeliveryPoint { get; set; }

        public int VolumetricWeight { get; set; }
    }
}