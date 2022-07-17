using System;
namespace Fleet.Core.Entities;

public class VehicleEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string LicencePlate { get; set; }
}

