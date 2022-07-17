using System;
using Fleet.Core.Entities;
using Fleet.Core.Errors;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;

namespace Fleet.UnitTest.HandlerTests
{
    public class CreateDeliveryCommandHandlerTests : ComponentTestBase<CreateDeliveryCommandHandler>
    {
        public CreateDeliveryCommandHandlerTests(ComponentFixtureBase fixture) : base(fixture)
        {
            SetupILoggerByCurrentService();
        }

        public async Task Handler_Should_Throw_VehicleNotExistException()
        {
            CreateDeliveryCommand command = new CreateDeliveryCommand
            {
                Plate = Guid.NewGuid().ToString()
            };


            await Assert.ThrowsAsync<VehicleNotExistException>((async () => await ClassUnderTest.Handle(command, new CancellationToken())));
        }

        public async Task Handler_Should_Throw_DeliveryPointNotExistException()
        {
            string plate = Guid.NewGuid().ToString();

            VehicleEntity vehicleEntity = new VehicleEntity
            {
                LicencePlate = plate
            };

            Context.Vehicles.InsertOne(vehicleEntity);

            CreateDeliveryCommand command = new CreateDeliveryCommand
            {
                Plate = plate
            };


            await Assert.ThrowsAsync<DeliveryPointNotExistException>((async () => await ClassUnderTest.Handle(command, new CancellationToken())));
        }

        [Fact]
        public async Task Handler_Should_Delivery_Should_Package_Unload()
        {
            string plate = Guid.NewGuid().ToString();
            Random rnd = new Random();
            int deliveryPoint = rnd.Next(0, 99999);
            string deliveryName = Guid.NewGuid().ToString();

            VehicleEntity vehicleEntity = new VehicleEntity
            {
                LicencePlate = plate
            };

            Context.Vehicles.InsertOne(vehicleEntity);

            string barcodeNumber = "P" + Guid.NewGuid().ToString();

            PackageEntity packageEntity = new PackageEntity
            {
                BagBarcode = null,
                Barcode = barcodeNumber,
                DeliveryPoint = deliveryPoint,
                ShipmentUnloadOption = ShiptmenUnloadOption.PackageNotInBag,
                VolumetricWeight = 1
            };


            Context.Packages.InsertOne(packageEntity);


            DeliveryPointEntity deliveryPointEntity = new DeliveryPointEntity
            {
                Name = deliveryName,
                Value = deliveryPoint,
                UnloadOptions = new List<ShiptmenUnloadOption>
                {
                    ShiptmenUnloadOption.PackageNotInBag
                }

            };

            Context.DeliveryPoints.InsertOne(deliveryPointEntity);

            CreateDeliveryCommand command = new CreateDeliveryCommand
            {
                Plate = plate,
                Route = new List<DeliveryRouteItem>
                {
                    new DeliveryRouteItem
                    {
                        DeliveryPoint = deliveryPoint,
                        Deliveries = new List<DeliveriesItem>
                        {
                            new DeliveriesItem
                            {
                                Barcode = barcodeNumber
                            }
                        }
                    }
                }
            };

            CreateDeliveryCommandResponseModel result = await ClassUnderTest.Handle(command, new CancellationToken());
            Assert.Equal((int)PackageStatuses.Unloaded, result.Route.FirstOrDefault().Deliveries.FirstOrDefault().State);
        }

        [Fact]
        public async Task Handler_Should_Delivery_Should_Package_Should_Still_Loaded()
        {
            Random rnd = new Random();
            int deliveryPoint = rnd.Next(0, 999999);
            string plate = Guid.NewGuid().ToString();

            string deliveryName = Guid.NewGuid().ToString();

            VehicleEntity vehicleEntity = new VehicleEntity
            {
                LicencePlate = plate
            };

            Context.Vehicles.InsertOne(vehicleEntity);

            string barcodeNumber = "P" + Guid.NewGuid().ToString();

            PackageEntity packageEntity = new PackageEntity
            {
                BagBarcode = "2",
                Barcode = barcodeNumber,
                DeliveryPoint = deliveryPoint,
                ShipmentUnloadOption = ShiptmenUnloadOption.PackageInBag,
                VolumetricWeight = 1
            };


            Context.Packages.InsertOne(packageEntity);


            DeliveryPointEntity deliveryPointEntity = new DeliveryPointEntity
            {
                Name = deliveryName,
                Value = deliveryPoint,
                UnloadOptions = new List<ShiptmenUnloadOption>
                {
                    ShiptmenUnloadOption.PackageNotInBag
                }

            };

            Context.DeliveryPoints.InsertOne(deliveryPointEntity);

            CreateDeliveryCommand command = new CreateDeliveryCommand
            {
                Plate = plate,
                Route = new List<DeliveryRouteItem>
                {
                    new DeliveryRouteItem
                    {
                        DeliveryPoint = deliveryPoint,
                        Deliveries = new List<DeliveriesItem>
                        {
                            new DeliveriesItem
                            {
                                Barcode = barcodeNumber
                            }
                        }
                    }
                }
            };

            CreateDeliveryCommandResponseModel result = await ClassUnderTest.Handle(command, new CancellationToken());
            Assert.Equal((int)PackageStatuses.Loaded, result.Route.FirstOrDefault().Deliveries.FirstOrDefault().State);
        }
    }
}

