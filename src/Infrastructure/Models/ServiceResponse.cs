using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Models;

public class ServiceResponse<T> : ServiceResponse
{
    public ServiceResponse(T data, HttpStatusCode statusCode, ProblemDetails? error = null)
        : base(statusCode, error)
    {
        Data = data;
    }

    public ServiceResponse(HttpStatusCode statusCode, ProblemDetails? error = null)
        : base(statusCode, error) { }

    public T? Data { get; }
}

public class ServiceResponse
{
    public ServiceResponse(HttpStatusCode statusCode, ProblemDetails? error = null)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public HttpStatusCode StatusCode { get; }
    public ProblemDetails? Error { get; }

    public bool IsSuccess => (int)StatusCode > 199 && (int)StatusCode < 300;
}
