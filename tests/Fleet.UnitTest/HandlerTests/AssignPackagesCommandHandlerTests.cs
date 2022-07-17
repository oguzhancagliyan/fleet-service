using System;
using Fleet.Core.Entities;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Fleet.UnitTest.HandlerTests
{
    public class AssignPackagesCommandHandlerTests : ComponentTestBase<AssignPackagesCommandHandlers>
    {
        public AssignPackagesCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        [Fact]
        public async Task Handler_Should_Throw_BagNotFoundException()
        {
            string barcode = Guid.NewGuid().ToString();
            string bagBarcode = Guid.NewGuid().ToString();

            AssignPackagesCommand command = new AssignPackagesCommand
            {
                Items = new List<AssignPackageItem>
                {
                    new AssignPackageItem
                    {
                        BagBarcode = bagBarcode,
                        Barcode = barcode
                    }
                }
            };


            CancellationToken cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<BagNotFoundException>((async () => await ClassUnderTest.Handle(command, cancellationToken)));
        }

        [Fact]
        public async Task Handler_Should_Throw_PackageNotFoundException()
        {
            string barcode = Guid.NewGuid().ToString();
            string bagBarcode = Guid.NewGuid().ToString();

            BagEntity entity = new BagEntity
            {
                Barcode = barcode,
                DeliveryPoint = 1
            };

            await Context.Bags.InsertOneAsync(entity);

            AssignPackagesCommand command = new AssignPackagesCommand
            {
                Items = new List<AssignPackageItem>
                {
                    new AssignPackageItem
                    {
                        BagBarcode = barcode,
                        Barcode = bagBarcode
                    }
                }
            };


            CancellationToken cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<PackageNotFoundException>((async () => await ClassUnderTest.Handle(command, cancellationToken)));
        }

        [Fact]
        public async Task Handler_Should_Throw_PackageAreadyInABagException()
        {
            string barcode = Guid.NewGuid().ToString();
            string bagBarcode = Guid.NewGuid().ToString();
            Random rnd = new Random();
            int deliveryPoint = rnd.Next(0, 99999);

            BagEntity bagEntity = new BagEntity
            {
                Barcode = bagBarcode,
                DeliveryPoint = deliveryPoint,
            };

            PackageEntity packageEntity = new PackageEntity
            {
                Barcode = barcode,
                BagBarcode = bagBarcode,
                DeliveryPoint = deliveryPoint,
                VolumetricWeight = 1
            };

            Context.Bags.InsertOne(bagEntity);

            Context.Packages.InsertOne(packageEntity);

            AssignPackagesCommand command = new AssignPackagesCommand
            {
                Items = new List<AssignPackageItem>
                {
                    new AssignPackageItem
                    {
                        BagBarcode = bagBarcode,
                        Barcode = barcode
                    }
                }
            };


            CancellationToken cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<PackageAreadyInABagException>((async () => await ClassUnderTest.Handle(command, cancellationToken)));
        }

        public async Task Handler_Should_Throw_PackageAndBagDeliveryPointIsDifferentException()
        {
            string barcode = Guid.NewGuid().ToString();
            string bagBarcode = Guid.NewGuid().ToString();
            Random rnd = new Random();
            int deliveryPoint = rnd.Next(0, 99999);
            int deliveryPoint2 = rnd.Next(0, 99999);

            BagEntity bagEntity = new BagEntity
            {
                Barcode = bagBarcode,
                DeliveryPoint = deliveryPoint,
            };

            PackageEntity packageEntity = new PackageEntity
            {
                Barcode = barcode,
                DeliveryPoint = deliveryPoint2,
                VolumetricWeight = 1
            };

            Context.Bags.InsertOne(bagEntity);

            Context.Packages.InsertOne(packageEntity);

            AssignPackagesCommand command = new AssignPackagesCommand
            {
                Items = new List<AssignPackageItem>
                {
                    new AssignPackageItem
                    {
                        BagBarcode = bagBarcode,
                        Barcode = barcode
                    }
                }
            };


            CancellationToken cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<PackageAndBagDeliveryPointIsDifferentException>((async () => await ClassUnderTest.Handle(command, cancellationToken)));
        }
    }
}

