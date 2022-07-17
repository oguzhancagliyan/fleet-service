using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests;

public class CreateDeliveryCommandValidatorTests : ComponentTestBase<CreateDeliveryCommandHandler>
{
    private readonly CreateDeliveryCommandValidator _deliveryCommandValidator;
    private readonly DeliveryRouteItemValidator _routeItemValidator;
    private readonly DeliveriesItemValidator _deliveryItemValidator;


    public CreateDeliveryCommandValidatorTests(ComponentFixtureBase fixture) : base(fixture)
    {
        _deliveryCommandValidator = new CreateDeliveryCommandValidator();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateDeliveryCommandValidator_Should_Not_Validate_For_Plate(string plate)
    {
        CreateDeliveryCommand command = new CreateDeliveryCommand
        {
            Plate = plate
        };

        var result = _deliveryCommandValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Plate);
    }

    [Fact]
    public void CreateDeliveryCommandValidator_Should_Not_Validate_For_Route()
    {
        CreateDeliveryCommand command = new CreateDeliveryCommand
        {
            Route = null
        };

        var result = _deliveryCommandValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Route);
    }


    [Fact]
    public void CreateDeliveryCommandValidator_Should_Validate()
    {
        CreateDeliveryCommand command = new CreateDeliveryCommand
        {
            Plate = "abc",
            Route = new List<DeliveryRouteItem>
            {
                new DeliveryRouteItem
                {
                    Deliveries = new List<DeliveriesItem>
                    {
                        new DeliveriesItem
                        {
                            Barcode = "ss"
                        }
                    },
                    DeliveryPoint = 12
                }
            }
        };

        var result = _deliveryCommandValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

