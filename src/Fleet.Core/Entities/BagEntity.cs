using System;
namespace Fleet.Core.Entities;

public class BagEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Barcode { get; set; }

    public int DeliveryPoint { get; set; }

    public BagStatuses Status { get; set; }

    public ShiptmenUnloadOption ShipmentUnloadOption { get; set; }
}

