namespace Fleet.Core.Entities
{
    public class FleetContext
    {
        private readonly IMongoDatabase _database = null;
        public readonly IMongoClient _client;

        public FleetContext(IMongoClient client)
        {
            _client = client;
            _database = _client.GetDatabase("FleetDb");
        }


        public IMongoCollection<DeliveryPointEntity> DeliveryPoints => _database.GetCollection<DeliveryPointEntity>("DeliveryPoint");

        public IMongoCollection<VehicleEntity> Vehicles => _database.GetCollection<VehicleEntity>("Vehicle");

        public IMongoCollection<BagEntity> Bags => _database.GetCollection<BagEntity>("Bag");

        public IMongoCollection<PackageEntity> Packages => _database.GetCollection<PackageEntity>("Package");
    }
}

