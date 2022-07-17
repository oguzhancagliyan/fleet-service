namespace Fleet.Core;
public class ModelValidator : IModelValidator
{
    private readonly IValidatorFactory _validatorFactory;

    public ModelValidator(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    [DebuggerStepThrough]
    public Task ValidateAndThrowAsync<T>(T model)
    {
        var validator = _validatorFactory.GetValidator<T>();
        return validator.ValidateAndThrowAsync<T>(model);
    }
}

