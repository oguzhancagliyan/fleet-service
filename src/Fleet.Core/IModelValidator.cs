namespace Fleet.Core;

public interface IModelValidator
{
    Task ValidateAndThrowAsync<T>(T model);
}