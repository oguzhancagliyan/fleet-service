using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests;

public class CreateBagCommandValidatorTests : ComponentTestBase<CreateBagCommandHandler>
{
    private readonly CreateBagCommandValidator _validator;
    private readonly BagItemsValidator _bagItemValidator;

    public CreateBagCommandValidatorTests(ComponentFixtureBase fixture) : base(fixture)
    {
        _validator = new CreateBagCommandValidator();
        _bagItemValidator = new BagItemsValidator();
    }


    [Theory]
    [InlineData("", 1)]
    [InlineData(" ", 1)]
    [InlineData(null, 1)]
    public void BagItemsValidator_Should_Not_Validate_For_Barcode(string barcode, int deliveryPoint)
    {
        BagItems command = new BagItems
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint
        };

        var result = _bagItemValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Barcode);
        result.ShouldNotHaveValidationErrorFor(c => c.DeliveryPoint);
    }

    [Theory]
    [InlineData("abc", 0)]
    public void BagItemsValidator_Should_Not_Validate_For_DeliveryPoint(string barcode, int deliveryPoint)
    {
        BagItems command = new BagItems
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint
        };

        var result = _bagItemValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DeliveryPoint);
        result.ShouldNotHaveValidationErrorFor(c => c.Barcode);
    }

    [Theory]
    [InlineData("abc", 1)]
    public void BagItemsValidator_Should_Validate(string barcode, int deliveryPoint)
    {
        BagItems command = new BagItems
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint
        };

        var result = _bagItemValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.DeliveryPoint);
        result.ShouldNotHaveValidationErrorFor(c => c.Barcode);
    }
}

