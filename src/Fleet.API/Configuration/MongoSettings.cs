using System;
namespace Fleet.API.Configuration;

public class MongoSettings
{
    public string ConnectionString { get; set; }

    public string DatabaseName { get; set; }
}
