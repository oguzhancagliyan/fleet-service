using AutoFixture;
using Fleet.Core;
using Fleet.Core.Entities;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.AutoFixture;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;
namespace Fleet.UnitTest.Base;

public abstract class ComponentTestBase<TComponent> : ComponentTestBase<TComponent, ComponentFixtureBase> where TComponent : class
{
    protected ComponentTestBase(ComponentFixtureBase fixture) : base(fixture)
    {
    }
}

public abstract class ComponentTestBase<TComponent, TFixture> : IClassFixture<TFixture>, IDisposable
      where TComponent : class
      where TFixture : ComponentFixtureBase
{
    protected IModelValidator ModelValidator;

    public MongoClient Client;

    public FleetContext Context;

    public MongoDbRunner Runner;

    protected readonly Fixture Fixture;
    private readonly Dictionary<Type, Mock> _injectedMocks;

    protected ComponentTestBase(TFixture fixture)
    {
        var componentFixtureBase = fixture as ComponentFixtureBase;
        ModelValidator = componentFixtureBase.ModelValidator;
        Client = (MongoClient)componentFixtureBase.Client;
        Context = componentFixtureBase.Context;
        Runner = componentFixtureBase.Runner;

        Fixture = new Fixture();
        Fixture.Customize(new StrictAutoMoqCustomization());
        Fixture.Inject(ModelValidator);
        Fixture.Inject(Context);
        Fixture.Inject(Client);
        _injectedMocks = new Dictionary<Type, Mock>();
    }

    public Mock<TMockType> MockFor<TMockType>() where TMockType : class
    {
        var key = typeof(TMockType);
        if (_injectedMocks.ContainsKey(key))
        {
            return _injectedMocks[key] as Mock<TMockType>;
        }
        var newMock = new Mock<TMockType>(MockBehavior.Strict);
        _injectedMocks.Add(key, newMock);
        Fixture.Inject(newMock.Object);

        return newMock;
    }

    public Mock<ILogger<TComponent>> SetupILoggerByCurrentService()
    {
        Mock<ILogger<TComponent>> mockFor = MockFor<ILogger<TComponent>>();

        mockFor.Setup(logger => logger.Log(It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return mockFor;
    }

    public Mock<ILogger> SetupILogger()
    {
        Mock<ILogger> mockFor = MockFor<ILogger>();

        mockFor.Setup(logger => logger.Log(It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return mockFor;
    }

    public Mock<ILogger<TComponent>> VerifyLogging(Mock<ILogger<TComponent>> logger, string expectedMessage = null, LogLevel expectedLogLevel = LogLevel.Debug, Times? times = null)
    {
        times ??= Times.Once();

        Func<object, Type, bool> state;

        if (!string.IsNullOrEmpty(expectedMessage))
        {
            state = (v, t) => string.Compare(v.ToString(), expectedMessage, StringComparison.Ordinal) == 0;
        }
        else
        {
            state = (v, t) => true;
        }

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), (Times)times);

        return logger;
    }

    public Mock<ILogger<TOtherService>> SetupILoggerWithOtherService<TOtherService>()
    {
        Mock<ILogger<TOtherService>> mockFor = MockFor<ILogger<TOtherService>>();

        mockFor.Setup(logger => logger.Log(It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

        return mockFor;
    }

    public TComponent ClassUnderTest => Fixture.Create<TComponent>();

    public void Dispose()
    {
        Runner.Dispose();
    }
}

public class ComponentFixtureBase
{
    public readonly IModelValidator ModelValidator;

    public readonly FleetContext Context;

    public readonly IMongoClient Client;

    public readonly MongoDbRunner Runner;

    public ComponentFixtureBase()
    {
        ModelValidator = new ModelValidator(GetFluentValidationValidatorFactory().Item1);
        Client = GetFluentValidationValidatorFactory().Item2;
        Context = GetFluentValidationValidatorFactory().Item3;
        Runner = GetFluentValidationValidatorFactory().Item4;
    }

    private static (IValidatorFactory, IMongoClient, FleetContext, MongoDbRunner) GetFluentValidationValidatorFactory()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddControllers()
            .AddFluentValidation(m =>
                m.RegisterValidatorsFromAssemblyContaining<AssignPackageItemValidator>());

        serviceCollection
            .AddLocalization(opts => { opts.ResourcesPath = "Resources"; })
            .AddLogging();

        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var validatorFactory = buildServiceProvider.GetRequiredService<IValidatorFactory>();

        MongoDbRunner mongoDbRunner = MongoDbRunner.StartForDebugging(singleNodeReplSet: true);

        MongoClientSettings dbSettings = MongoClientSettings.FromConnectionString(MongoDbRunner.StartForDebugging(singleNodeReplSet: true).ConnectionString);
        dbSettings.SslSettings.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        var client = new MongoClient(dbSettings);
        if (mongoDbRunner.State != State.AlreadyRunning)
        {
            CreateCollectionsAndSeed(client);
        }

        FleetContext context = new FleetContext(client);

        serviceCollection.AddSingleton<IMongoClient>(client);

        return (validatorFactory, client, context, mongoDbRunner);
    }

    private static void CreateCollectionsAndSeed(MongoClient client)
    {
        var db = client.GetDatabase("FleetDb");

        string[] tables = { "DeliveryPoint", "Vehicle", "Package", "Bag" };

        foreach (string table in tables)
        {
            try
            {
                db.DropCollection(table);
                Console.WriteLine("DropCollection");
            }
            catch (Exception e)
            {
                Console.WriteLine("DropCollection catch");
            }
            db.CreateCollection(table);
            Console.WriteLine("DropCollection");
        }
    }
}
