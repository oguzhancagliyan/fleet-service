using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests;

public class CreatePackageCommandValidatorTests : ComponentTestBase<CreatePackageCommandHandler>
{
    private readonly PackageItemValidator _validator;
    private readonly CreatePackageCommandValidator _packageValidator;

    public CreatePackageCommandValidatorTests(ComponentFixtureBase fixture) : base(fixture)
    {
        _validator = new PackageItemValidator();
        _packageValidator = new CreatePackageCommandValidator();
    }

    [Theory]
    [InlineData("", 1, 1)]
    [InlineData(" ", 1, 1)]
    [InlineData(null, 1, 1)]
    public void PackageItemValidator_Should_Not_Validate_For_Barcode(string barcode, int deliveryPoint, int volumetricWeight)
    {
        PackageItem command = new PackageItem
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint,
            VolumetricWeight = volumetricWeight
        };


        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Barcode);
        result.ShouldNotHaveValidationErrorFor(c => c.DeliveryPoint);
        result.ShouldNotHaveValidationErrorFor(c => c.VolumetricWeight);
    }

    [Theory]
    [InlineData("ab", 0, 1)]
    public void PackageItemValidator_Should_Not_Validate_For_DeliveryPoint(string barcode, int deliveryPoint, int volumetricWeight)
    {
        PackageItem command = new PackageItem
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint,
            VolumetricWeight = volumetricWeight
        };


        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.DeliveryPoint);
        result.ShouldNotHaveValidationErrorFor(c => c.Barcode);
        result.ShouldNotHaveValidationErrorFor(c => c.VolumetricWeight);
    }

    [Theory]
    [InlineData("ab", 1, 0)]
    public void PackageItemValidator_Should_Not_Validate_For_Volumetric(string barcode, int deliveryPoint, int volumetricWeight)
    {
        PackageItem command = new PackageItem
        {
            Barcode = barcode,
            DeliveryPoint = deliveryPoint,
            VolumetricWeight = volumetricWeight
        };


        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.VolumetricWeight);
        result.ShouldNotHaveValidationErrorFor(c => c.Barcode);
        result.ShouldNotHaveValidationErrorFor(c => c.DeliveryPoint);
    }

    public void CreatePackageCommandValidator_Should_Not_Validate()
    {
        CreatePackageCommand command = new CreatePackageCommand
        {
            Items = null
        };

        var result = _packageValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Items);
    }
}

