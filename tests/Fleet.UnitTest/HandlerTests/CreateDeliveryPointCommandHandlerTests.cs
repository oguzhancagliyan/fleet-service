using System;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;

namespace Fleet.UnitTest.HandlerTests
{
    public class CreateDeliveryPointCommandHandlerTests : ComponentTestBase<CreateDeliveryPointCommandHandler>
    {
        public CreateDeliveryPointCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        [Fact]
        public async Task Handler_Should_Throw_DeliveryPointExistException()
        {
            string name = "deliverypointExist";
            int value = 1000000;

            await Context.DeliveryPoints.InsertOneAsync(new Core.Entities.DeliveryPointEntity
            {
                Name = name,
                Value = value
            }, null, new CancellationToken());

            CreateDeliveryPointCommand command = new CreateDeliveryPointCommand
            {
                Name = name,
                Value = value
            };

            await Assert.ThrowsAsync<DeliveryPointExistException>((async () => await ClassUnderTest.Handle(command, new CancellationToken())));
        }
    }
}

