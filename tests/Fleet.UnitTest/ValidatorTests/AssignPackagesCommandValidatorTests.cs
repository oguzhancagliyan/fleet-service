using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests;

public class AssignPackagesCommandValidatorTests : ComponentTestBase<AssignPackagesCommandHandlers>
{
    private readonly AssignPackagesCommandValidator _validator;
    private readonly AssignPackageItemValidator _packageItemValidator;

    public AssignPackagesCommandValidatorTests(ComponentFixtureBase fixture) : base(fixture)
    {
        _validator = new AssignPackagesCommandValidator();
        _packageItemValidator = new AssignPackageItemValidator();
    }

    [Fact]
    public void AssignPackagesCommandValidator_Should_Not_Validate()
    {
        AssignPackagesCommand command = new AssignPackagesCommand
        {
            Items = null
        };

        var resultForNullList = _validator.TestValidate(command);

        AssignPackagesCommand command2 = new AssignPackagesCommand
        {
            Items = new List<AssignPackageItem>()
        };

        var resultForEmptyList = _validator.TestValidate(command2);

        resultForEmptyList.ShouldHaveValidationErrorFor(c => c.Items);
        resultForNullList.ShouldHaveValidationErrorFor(c => c.Items);
    }

    [Fact]
    public void AssignPackagesCommandValidator_Should_Validate()
    {
        AssignPackagesCommand command = new AssignPackagesCommand
        {
            Items = new List<AssignPackageItem>
            {
                new AssignPackageItem
                {
                    BagBarcode = "abc",
                    Barcode = "abc"
                }
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "abc")]
    [InlineData(" ", "abc")]
    [InlineData(null, "abc")]
    public void AssignPackageItemValidator_Should_Not_Validate_For_Barcode(string barcode, string bagbarcode)
    {
        AssignPackageItem command = new AssignPackageItem
        {
            Barcode = barcode,
            BagBarcode = bagbarcode
        };

        var result = _packageItemValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Barcode);
    }

    [Theory]
    [InlineData("", "abc")]
    [InlineData(" ", "abc")]
    [InlineData(null, "abc")]
    public void AssignPackageItemValidator_Should_Not_Validate_For_Bagbarcode(string bagbarcode, string barcode)
    {
        AssignPackageItem command = new AssignPackageItem
        {
            Barcode = barcode,
            BagBarcode = bagbarcode
        };

        var result = _packageItemValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.BagBarcode);
    }

    [Theory]
    [InlineData("abc", "abc")]
    public void AssignPackageItemValidator_Should_Validate(string bagbarcode, string barcode)
    {
        AssignPackageItem command = new AssignPackageItem
        {
            Barcode = barcode,
            BagBarcode = bagbarcode
        };

        var result = _packageItemValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

