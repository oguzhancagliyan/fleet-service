using System;
namespace Fleet.Core.Errors;

public class ErrorMessage
{
    public string Message { get; set; }

    public IList<ErrorDetail> ErrorDetails { get; set; }
}

public class ErrorDetail
{
    public string ErrorCode { get; set; }

    public string PropertyName { get; set; }
}

