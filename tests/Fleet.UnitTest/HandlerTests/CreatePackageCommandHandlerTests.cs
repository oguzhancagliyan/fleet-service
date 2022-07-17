using System;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;

namespace Fleet.UnitTest.HandlerTests
{
    public class CreatePackageCommandHandlerTests : ComponentTestBase<CreatePackageCommandHandler>
    {
        public CreatePackageCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        public async Task Handler_Should_Throw_DeliveryPointNotExistException()
        {
            CreatePackageCommand command = new CreatePackageCommand
            {
                Items = new List<PackageItem>
                {
                    new PackageItem
                    {
                        Barcode = "abc",
                        DeliveryPoint = 1,
                        VolumetricWeight = 1
                    }
                }
            };


            await Assert.ThrowsAsync<DeliveryPointNotExistException>((async () => await ClassUnderTest.Handle(command, new CancellationToken())));
        }
    }
}

