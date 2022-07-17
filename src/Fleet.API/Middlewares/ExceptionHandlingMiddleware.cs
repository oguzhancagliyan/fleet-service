using System;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Fleet.Core.Errors;
using Fleet.Core.Extensions;

namespace Fleet.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                (ErrorMessage error, HttpStatusCode statusCode) = HandleException(e);

                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var serializedError = JsonSerializer.Serialize(error);
                await context.Response.WriteJsonAsync(serializedError);
            }
        }

        private (ErrorMessage error, HttpStatusCode statusCode) HandleException(Exception exception)
        {

            var httpStatusCode = HttpStatusCode.InternalServerError;

            var errorMessage = new ErrorMessage
            {
                Message = "Error occured"
            };

            if (httpStatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "{@ErrorMessage} {HttpStatusCode}", errorMessage, httpStatusCode);
            }
            else
            {
                _logger.LogWarning(exception, "{@ErrorMessage} {HttpStatusCode}", errorMessage, httpStatusCode);
            }

            return (errorMessage, httpStatusCode);
        }

    }
}

