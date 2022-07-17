using System;

namespace Fleet.Core.Handlers.CommandHandlers;

public class AssignPackagesCommandHandlers : IRequestHandler<AssignPackagesCommand>
{
    private readonly FleetContext _context;
    private readonly ILogger<AssignPackagesCommandHandlers> _logger;

    public AssignPackagesCommandHandlers(FleetContext context, ILogger<AssignPackagesCommandHandlers> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(AssignPackagesCommand request, CancellationToken cancellationToken = default)
    {
        List<string> bagBarcodeList = request.Items.GroupBy(c => c.BagBarcode).Select(cd => cd.Key).ToList();
        var bagBarcodeFilter = Builders<BagEntity>.Filter.In(c => c.Barcode, bagBarcodeList);

        List<BagEntity> bagEntities = await _context.Bags.Find(bagBarcodeFilter).ToListAsync(cancellationToken);

        if (!bagEntities.Any() || bagEntities.Count != bagBarcodeList.Count)
        {
            _logger.LogError("Someone tried to assign nonexisted bag. Request : {@request}", request);
            throw new BagNotFoundException();
        }

        List<string> packageBarcodeList = request.Items.GroupBy(c => c.Barcode).Select(cd => cd.Key).ToList();
        var packageBarcodeFilter = Builders<PackageEntity>.Filter.In(c => c.Barcode, packageBarcodeList);


        List<PackageEntity> packageEntities = await _context.Packages.Find(packageBarcodeFilter).ToListAsync(cancellationToken);


        if (!packageEntities.Any() || packageEntities.Count != packageBarcodeList.Count)
        {
            _logger.LogError("Someone tried to assign nonexisted package. Request : {@request}", request);
            throw new PackageNotFoundException();
        }

        if (packageEntities.Any(c => c.BagBarcode != null))
        {
            //TODO: should we ask to PM/PO to Should we check is package has already in a bag.
            _logger.LogError("Someone tried to assign a package which already in a bag to another bag. Filled packages : {@packages}", packageEntities.Where(c => c.BagBarcode != null).ToList());
            throw new PackageAreadyInABagException();
        }


        ValidateBagAndPackageDeliveryPoint(request.Items, bagEntities, packageEntities);

        try
        {
            foreach (PackageEntity entity in packageEntities)
            {
                _logger.LogInformation("Process entity is : {@entity}", entity);
                var barcode = request.Items.FirstOrDefault(c => c.Barcode == entity.Barcode).BagBarcode;
                var updateDefinition = Builders<PackageEntity>.
                    Update.
                    Set(nameof(PackageEntity.BagBarcode), barcode).
                    Set(nameof(PackageEntity.ShipmentUnloadOption), ShiptmenUnloadOption.PackageInBag).
                    Set(nameof(PackageEntity.Status), PackageStatuses.LoadedIntoBag);

                await _context.Packages.UpdateOneAsync(a => a.Id == entity.Id, updateDefinition, null, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured. Request is {@request}", request);

            throw ex;
        }

        return Unit.Value;
    }

    private void ValidateBagAndPackageDeliveryPoint(List<AssignPackageItem> items, List<BagEntity> bagEntities, List<PackageEntity> packageEntities)
    {
        foreach (var item in items)
        {
            var currentBag = bagEntities.FirstOrDefault(c => c.Barcode == item.BagBarcode);

            var currentPackage = packageEntities.FirstOrDefault(c => c.Barcode == item.Barcode);

            if (currentBag.DeliveryPoint != currentPackage.DeliveryPoint)
            {
                _logger.LogError("bag and package have different deliverypoint. bag : {@bag} package : {@package}", currentBag, currentPackage);
                throw new PackageAndBagDeliveryPointIsDifferentException();
            }
        }
    }
}

public record AssignPackagesCommand : IRequest
{
    public List<AssignPackageItem> Items { get; init; }
}

public record AssignPackageItem
{
    public string Barcode { get; init; }

    public string BagBarcode { get; init; }
}

public class AssignPackagesCommandValidator : AbstractValidator<AssignPackagesCommand>
{
    public AssignPackagesCommandValidator()
    {
        RuleFor(c => c.Items).NotEmpty();
        RuleForEach(c => c.Items).SetValidator(new AssignPackageItemValidator());
    }
}

public class AssignPackageItemValidator : AbstractValidator<AssignPackageItem>
{
    public AssignPackageItemValidator()
    {
        RuleFor(c => c.Barcode).NotEmpty();
        RuleFor(c => c.BagBarcode).NotEmpty();
    }
}