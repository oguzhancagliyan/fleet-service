using System;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;


namespace Fleet.UnitTest.HandlerTests
{
    public class CreateBagCommandHandlerTests : ComponentTestBase<CreateBagCommandHandler>
    {
        public CreateBagCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        [Fact]
        public async Task Handler_Should_Throw_DeliveryPointNotExistException()
        {
            string barcode = Guid.NewGuid().ToString();
            Random rnd = new Random();
            int deliveryPoint = rnd.Next(0, 99999);
            CreateBagCommand command = new CreateBagCommand
            {
                Items = new List<BagItems>
                {
                    new BagItems
                    {
                        Barcode = barcode,
                        DeliveryPoint = deliveryPoint
                    }
                }
            };

            CancellationToken cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<DeliveryPointNotExistException>((async () => await ClassUnderTest.Handle(command, cancellationToken)));
        }
    }
}

