using System;
using System.Text.Json.Serialization;

namespace Fleet.Core.Handlers.CommandHandlers;

public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, CreateDeliveryCommandResponseModel>
{
    private readonly FleetContext _context;
    private readonly ILogger<CreateDeliveryCommandHandler> _logger;

    public CreateDeliveryCommandHandler(FleetContext context, ILogger<CreateDeliveryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateDeliveryCommandResponseModel> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
    {
        VehicleEntity currentVehicle = await _context.Vehicles.Find(c => c.LicencePlate == request.Plate).FirstOrDefaultAsync(cancellationToken);

        if (currentVehicle == null)
        {
            throw new VehicleNotExistException();
        }

        List<int> deliveryPoints = request.Route.GroupBy(c => c.DeliveryPoint).Select(cd => cd.Key).ToList();
        var deliveryPointFilter = Builders<DeliveryPointEntity>.Filter.In(c => c.Value, deliveryPoints);

        List<DeliveryPointEntity> deliveryPointEntities = await _context.DeliveryPoints.Find(deliveryPointFilter).ToListAsync(cancellationToken);

        if (!deliveryPointEntities.Any() || deliveryPointEntities.Count != deliveryPoints.Count)
        {
            _logger.LogError("Someone tried to delivery nonexisted deliverypoint. Request : {@request}", request);
            throw new DeliveryPointNotExistException();
        }

        List<CreateDeliveryCommandRouteModel> createDeliveryCommandRouteModels = new List<CreateDeliveryCommandRouteModel>();

        CreateDeliveryCommandResponseModel responseModel = new CreateDeliveryCommandResponseModel
        {
            Plate = request.Plate,
            Route = createDeliveryCommandRouteModels
        };

        foreach (var route in request.Route)
        {
            List<CreateDeliveryCommandDeliveriesModel> createDeliveryCommandDeliveriesModels = new List<CreateDeliveryCommandDeliveriesModel>();

            foreach (var delivery in route.Deliveries)
            {
                //should we assume bags start with C and packages start with P.
                //I'll assume for a now
                if (delivery.Barcode.StartsWith("P"))
                {
                    //package
                    PackageEntity currentPackage = await _context.Packages.Find(c => c.Barcode == delivery.Barcode).FirstOrDefaultAsync(cancellationToken);

                    if (currentPackage == null)
                    {
                        _logger.LogError("Someone tried to delivery nonexisted package");
                        continue;
                    }

                    var packageStatus = PackageStatuses.Loaded;

                    var updateDefinition = Builders<PackageEntity>.
                        Update.
                        Set(nameof(PackageEntity.Status), packageStatus);

                    await _context.Packages.UpdateOneAsync(a => a.Id == currentPackage.Id, updateDefinition, null, cancellationToken);

                    bool shouldUnload = ShouldPackageUnloadToDeliveryPoint(deliveryPointEntities.FirstOrDefault(c => c.Value == route.DeliveryPoint), currentPackage);

                    if (shouldUnload)
                    {
                        packageStatus = PackageStatuses.Unloaded;
                        updateDefinition = Builders<PackageEntity>.
                       Update.
                       Set(nameof(PackageEntity.Status), packageStatus);

                        await _context.Packages.UpdateOneAsync(a => a.Id == currentPackage.Id, updateDefinition, null, cancellationToken);
                    }

                    createDeliveryCommandDeliveriesModels.Add(new CreateDeliveryCommandDeliveriesModel
                    {
                        State = (int)packageStatus,
                        Barcode = delivery.Barcode
                    });
                }
                else if (delivery.Barcode.StartsWith("C"))
                {
                    //bag
                    BagEntity currentBag = await _context.Bags.Find(c => c.Barcode == delivery.Barcode).FirstOrDefaultAsync(cancellationToken);

                    if (currentBag == null)
                    {
                        _logger.LogError("Someone tried to delivery nonexisted bag");
                        continue;
                    }

                    var packageStatus = PackageStatuses.Loaded;

                    var updateDefinition = Builders<BagEntity>.
                       Update.
                       Set(nameof(PackageEntity.Status), packageStatus);

                    await _context.Bags.UpdateOneAsync(a => a.Id == currentBag.Id, updateDefinition, null, cancellationToken);

                    bool shouldUnload = ShouldBagUnloadToDeliveryPoint(deliveryPointEntities.FirstOrDefault(c => c.Value == route.DeliveryPoint), currentBag);

                    if (shouldUnload)
                    {
                        packageStatus = PackageStatuses.Unloaded;
                        updateDefinition = Builders<BagEntity>.
                       Update.
                       Set(nameof(PackageEntity.Status), packageStatus);

                        await _context.Bags.UpdateOneAsync(a => a.Id == currentBag.Id, updateDefinition, null, cancellationToken);
                    }

                    createDeliveryCommandDeliveriesModels.Add(new CreateDeliveryCommandDeliveriesModel
                    {
                        State = (int)packageStatus,
                        Barcode = delivery.Barcode
                    });
                }
            }

            CreateDeliveryCommandRouteModel createDeliveryCommandRouteModel = new CreateDeliveryCommandRouteModel
            {
                DeliveryPoint = route.DeliveryPoint,
                Deliveries = createDeliveryCommandDeliveriesModels
            };

            createDeliveryCommandRouteModels.Add(createDeliveryCommandRouteModel);
        }
        return responseModel;
    }

    private bool ShouldPackageUnloadToDeliveryPoint(DeliveryPointEntity deliveryPointEntity, PackageEntity entity)
    {
        if (entity.DeliveryPoint != deliveryPointEntity.Value)
        {
            return false;
        }

        if (deliveryPointEntity.UnloadOptions.Contains(entity.ShipmentUnloadOption))
        {
            return true;
        }

        return false;

    }

    private bool ShouldBagUnloadToDeliveryPoint(DeliveryPointEntity deliveryPointEntity, BagEntity entity)
    {
        if (entity.DeliveryPoint != deliveryPointEntity.Value)
        {
            return false;
        }

        if (deliveryPointEntity.UnloadOptions.Contains(entity.ShipmentUnloadOption))
        {
            return true;
        }

        return false;
    }
}

public record CreateDeliveryCommand : IRequest<CreateDeliveryCommandResponseModel>
{
    [JsonPropertyName(name: "plate")]
    public string Plate { get; init; }

    [JsonPropertyName(name: "route")]
    public List<DeliveryRouteItem> Route { get; init; }
}

public class CreateDeliveryCommandValidator : AbstractValidator<CreateDeliveryCommand>
{
    public CreateDeliveryCommandValidator()
    {
        RuleFor(c => c.Plate).NotEmpty();
        RuleFor(c => c.Route).NotEmpty();
        RuleForEach(c => c.Route).SetValidator(new DeliveryRouteItemValidator());
    }
}

public record DeliveryRouteItem
{
    [JsonPropertyName(name: "deliveryPoint")]
    public int DeliveryPoint { get; init; }

    [JsonPropertyName(name: "deliveries")]
    public List<DeliveriesItem> Deliveries { get; init; }
}

public class DeliveryRouteItemValidator : AbstractValidator<DeliveryRouteItem>
{
    public DeliveryRouteItemValidator()
    {
        RuleFor(c => c.DeliveryPoint).NotEmpty();
        RuleFor(c => c.Deliveries).NotEmpty();
        RuleForEach(c => c.Deliveries).SetValidator(new DeliveriesItemValidator());
    }
}

public record DeliveriesItem
{
    [JsonPropertyName(name: "barcode")]
    public string Barcode { get; init; }
}

public class DeliveriesItemValidator : AbstractValidator<DeliveriesItem>
{
    public DeliveriesItemValidator()
    {
        RuleFor(c => c.Barcode).NotEmpty();
    }
}

public record CreateDeliveryCommandResponseModel
{
    [JsonPropertyName(name: "plate")]
    public string Plate { get; init; }

    [JsonPropertyName(name: "route")]
    public List<CreateDeliveryCommandRouteModel> Route { get; init; }
}

public record CreateDeliveryCommandRouteModel
{
    [JsonPropertyName(name: "deliveryPoint")]
    public int DeliveryPoint { get; init; }

    [JsonPropertyName(name: "deliveries")]
    public List<CreateDeliveryCommandDeliveriesModel> Deliveries { get; init; }
}

public record CreateDeliveryCommandDeliveriesModel
{
    [JsonPropertyName(name: "barcode")]
    public string Barcode { get; set; }

    [JsonPropertyName(name: "state")]
    public int State { get; set; }
}
