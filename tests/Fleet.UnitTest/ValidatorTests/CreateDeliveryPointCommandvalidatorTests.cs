using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests;

public class CreateDeliveryPointCommandvalidatorTests : ComponentTestBase<CreateDeliveryPointCommandHandler>
{
    private readonly CreateDeliveryPointCommandvalidator _validator;


    public CreateDeliveryPointCommandvalidatorTests(ComponentFixtureBase fixture) : base(fixture)
    {
        _validator = new CreateDeliveryPointCommandvalidator();
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData(" ", 1)]
    [InlineData(null, 1)]
    public void CreateDeliveryPointCommandvalidator_Should_Not_Validate_For_Name(string name, int value)
    {
        CreateDeliveryPointCommand command = new CreateDeliveryPointCommand
        {
            Name = name,
            Value = value
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData("abc", 0)]
    [InlineData("ccc", -1)]
    public void CreateDeliveryPointCommandvalidator_Should_Not_Validate_For_Value(string name, int value)
    {
        CreateDeliveryPointCommand command = new CreateDeliveryPointCommand
        {
            Name = name,
            Value = value
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Value);
    }

    [Theory]
    [InlineData("abc", 1)]
    public void CreateDeliveryPointCommandvalidator_Should_Validate(string name, int value)
    {
        CreateDeliveryPointCommand command = new CreateDeliveryPointCommand
        {
            Name = name,
            Value = value
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}

