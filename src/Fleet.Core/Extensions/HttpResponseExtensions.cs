using System;
using Microsoft.AspNetCore.Http;

namespace Fleet.Core.Extensions;

public static class HttpResponseExtensions
{
    public static Task WriteJsonAsync(this HttpResponse response, string json)
    {
        response.ContentType = "application/json; charset=UTF-8";
        return response.WriteAsync(json);
    }
}

