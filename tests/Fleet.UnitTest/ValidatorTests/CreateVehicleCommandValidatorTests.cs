using System;
using Fleet.Core.Handlers.CommandHandlers;
using Fleet.UnitTest.Base;
using FluentValidation.TestHelper;

namespace Fleet.UnitTest.ValidatorTests
{
    public class CreateVehicleCommandValidatorTests : ComponentTestBase<CreateVehicleCommandHandler>
    {
        private readonly CreateVehicleCommandValidator _validator;

        public CreateVehicleCommandValidatorTests(ComponentFixtureBase fixture) : base(fixture)
        {
            _validator = new CreateVehicleCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void CreateVehicleCommandValidator_Should_Not_Validate(string licencePlate)
        {
            CreateVehicleCommand command = new CreateVehicleCommand()
            {
                LicencePlate = licencePlate
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveAnyValidationError();
        }


        [Theory]
        [InlineData("abc")]
        public void CreateVehicleCommandValidator_Should_Validate(string licencePlate)
        {
            CreateVehicleCommand command = new CreateVehicleCommand()
            {
                LicencePlate = licencePlate
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}