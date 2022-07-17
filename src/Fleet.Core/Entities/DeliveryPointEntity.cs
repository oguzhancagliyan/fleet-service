using System;
namespace Fleet.Core.Entities;

public class DeliveryPointEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Name { get; set; }

    public int Value { get; set; }

    public List<ShiptmenUnloadOption> UnloadOptions { get; set; }
}