using System;
namespace Fleet.Core.Entities;

public class PackageEntity
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Barcode { get; set; }

    public int DeliveryPoint { get; set; }

    public int VolumetricWeight { get; set; }

    public PackageStatuses Status { get; set; }

    public string BagBarcode { get; set; }

    public ShiptmenUnloadOption ShipmentUnloadOption { get; set; }
}

