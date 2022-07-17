using System;
using Fleet.Core.Entities;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;

namespace Fleet.UnitTest.HandlerTests
{
    public class CreateVecihleCommandHandlerTests : ComponentTestBase<CreateVehicleCommandHandler>
    {
        public CreateVecihleCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        public async Task Handler_Should_Throw_VehicleExistException()
        {

            string licencePlate = Guid.NewGuid().ToString();

            await Context.Vehicles.InsertOneAsync(new VehicleEntity
            {
                LicencePlate = licencePlate
            }, null, new CancellationToken());

            CreateVehicleCommand command = new CreateVehicleCommand
            {
                LicencePlate = licencePlate
            };

            await Assert.ThrowsAsync<VehicleExistException>((async () => await ClassUnderTest.Handle(command, new CancellationToken())));
        }
    }
}

