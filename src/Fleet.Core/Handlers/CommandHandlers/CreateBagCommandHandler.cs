using System;

namespace Fleet.Core.Handlers.CommandHandlers;

public class CreateBagCommandHandler : IRequestHandler<CreateBagCommand>
{
    private readonly FleetContext _context;
    private readonly ILogger<CreateBagCommandHandler> _logger;

    public CreateBagCommandHandler(FleetContext context, ILogger<CreateBagCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateBagCommand request, CancellationToken cancellationToken)
    {
        List<int> deliveryPointList = request.Items.GroupBy(c => c.DeliveryPoint).Select(cd => cd.Key).ToList();
        var deliveryPointFilter = Builders<DeliveryPointEntity>.Filter.In(c => c.Value, deliveryPointList);

        long deliveryPointCount = await _context.DeliveryPoints.Find(deliveryPointFilter).CountDocumentsAsync(cancellationToken: cancellationToken);

        if (deliveryPointCount != deliveryPointList.Count())
        {
            _logger.LogWarning("Someone tried to create a bad with nonexisted delivery point. Request : {@request}", request);

            throw new DeliveryPointNotExistException();
        }

        var entityList = request.Items.Adapt<List<BagEntity>>();

        await _context.Bags.InsertManyAsync(entityList, null, cancellationToken);

        return Unit.Value;
    }
}

public record CreateBagCommand : IRequest
{
    public List<BagItems> Items { get; init; }
}

public record BagItems
{
    public string Barcode { get; init; }

    public int DeliveryPoint { get; init; }
}

public class CreateBagCommandValidator : AbstractValidator<CreateBagCommand>
{
    public CreateBagCommandValidator()
    {
        RuleFor(c => c.Items).NotEmpty();
        RuleForEach(c => c.Items).SetValidator(new BagItemsValidator());
    }
}

public class BagItemsValidator : AbstractValidator<BagItems>
{
    public BagItemsValidator()
    {
        RuleFor(c => c.Barcode).NotEmpty();
        RuleFor(c => c.DeliveryPoint).GreaterThan(0);
    }
}
