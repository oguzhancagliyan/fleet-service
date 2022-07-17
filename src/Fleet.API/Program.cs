using System.Text.Json;
using Fleet.API.Configuration;
using Fleet.Core;
using Fleet.Core.Entities;
using Fleet.Core.PipelineBehaviors;
using FluentValidation.AspNetCore;
using Mapster;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddMediatR(typeof(AssignPackagesCommandHandlers).Assembly);
builder.Services.AddScoped<IModelValidator, ModelValidator>();
builder.Services.AddFluentValidation(configuration =>
{
    configuration.RegisterValidatorsFromAssemblyContaining<AssignPackageItemValidator>();
});

builder.Services.AddSingleton<FleetContext>();
builder.Services.Configure<MongoSettings>(options =>
{
    options.ConnectionString
        = builder.Configuration.GetSection("MongoConnection:ConnectionString").Value;
    options.DatabaseName
        = builder.Configuration.GetSection("MongoConnection:Database").Value;
});

builder.Services.AddSingleton<IMongoClient>(c =>
{
    IOptions<MongoSettings> settings = c.GetRequiredService<IOptions<MongoSettings>>();
    MongoClientSettings dbSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);
    dbSettings.SslSettings.ServerCertificateValidationCallback =
        (sender, certificate, chain, sslPolicyErrors) => true;
    return new MongoClient(dbSettings);
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .SetIsOriginAllowed((host) => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Host.UseSerilog((context, configuration) =>
{
    var applicationName = context.Configuration.GetSection("ApplicationName").Value;
    var environment = context.HostingEnvironment.EnvironmentName;
    var elasticIndexEnvironment = "development";

    var indexFormat = $"log-{elasticIndexEnvironment}-{applicationName}-{{0:yyyy.MM.dd}}";

    var elasticSearchUri = context.Configuration.GetSection("ElasticConfiguration:Uri").Value;
    var elasticSearchSinkOptions = new ElasticsearchSinkOptions(new Uri(elasticSearchUri))
    {
        AutoRegisterTemplate = true,
        IndexFormat = indexFormat,
        BufferCleanPayload = (failingEvent, statusCode, exception) =>
        {
            dynamic e = JsonDocument.Parse(failingEvent);
            return JsonSerializer.Serialize(new Dictionary<string, object>()
            {
                { "@timestamp", e["@timestamp"] },
                { "level", "Error" },
                { "message","Error: " + e.message },
                { "messageTemplate", e.messageTemplate },
                { "failingStatusCode", statusCode },
                { "failingException", exception }
            });
        },
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
        //EmitEventFailure = EmitEventFailureHandling.RaiseCallback,
        //FailureCallback = @event =>
        //{
        //    Console.WriteLine($"es write error -> [{@event.Exception?.GetType()?.Name}]: {@event.Exception?.Message}");
        //},
        FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                           EmitEventFailureHandling.WriteToFailureSink |
                           EmitEventFailureHandling.RaiseCallback,
        FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)
    };

    configuration
        .Enrich
        .FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Elasticsearch(elasticSearchSinkOptions);
});

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



TypeAdapterConfig<BagItems, BagEntity>
              .NewConfig()
              .Map(d => d.ShipmentUnloadOption, de => ShiptmenUnloadOption.Bag)
              .Map(d => d.Status, de => BagStatuses.Created);


TypeAdapterConfig<PackageItem, PackageEntity>
              .NewConfig()
              .Map(dest => dest.ShipmentUnloadOption, de => ShiptmenUnloadOption.PackageNotInBag)
              .Map(dest => dest.Status, de => PackageStatuses.Created);


app.Run();

