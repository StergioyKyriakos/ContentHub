using System.Net;

namespace ContentHub.Data.Entities.Common;

public sealed class DomainException : Exception
{
    public string Code { get; }

    public HttpStatusCode StatusCode { get; }

    public DomainException(
        string code,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public DomainException(
        DomainError error,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(error.Message)
    {
        Code = error.Code;
        StatusCode = statusCode;
    }
}