using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;

namespace HetznerCloud.Exceptions;

public class HetznerCloudException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }
    public IReadOnlyList<ErrorDetail> Details { get; }

    public HetznerCloudException(string message, HttpStatusCode statusCode, string errorCode = "", IEnumerable<ErrorDetail>? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = (details as IReadOnlyList<ErrorDetail>) ?? new List<ErrorDetail>(details ?? []).AsReadOnly();
    }
}

public class ErrorDetail
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ErrorResponse
{
    [JsonPropertyName("error")]
    public ErrorDetail Error { get; set; } = new();

    [JsonPropertyName("details")]
    public List<ErrorDetail> Details { get; set; } = [];
}

public class RateLimitExceededException : HetznerCloudException
{
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(string message, TimeSpan retryAfter, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.TooManyRequests, "rate_limit_exceeded", details)
    {
        RetryAfter = retryAfter;
    }
}

public class NotFoundException : HetznerCloudException
{
    public NotFoundException(string message, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.NotFound, "not_found", details)
    {
    }
}

public class UnauthorizedException : HetznerCloudException
{
    public UnauthorizedException(string message, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.Unauthorized, "unauthorized", details)
    {
    }
}

public class ForbiddenException : HetznerCloudException
{
    public ForbiddenException(string message, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.Forbidden, "forbidden", details)
    {
    }
}

public class ValidationException : HetznerCloudException
{
    public IReadOnlyList<ErrorDetail> ValidationErrors { get; }

    public ValidationException(string message, IEnumerable<ErrorDetail> validationErrors)
        : base(message, HttpStatusCode.UnprocessableEntity, "validation_error", validationErrors)
    {
        ValidationErrors = (validationErrors as IReadOnlyList<ErrorDetail>) ?? new List<ErrorDetail>(validationErrors).AsReadOnly();
    }
}

public class ConflictException : HetznerCloudException
{
    public ConflictException(string message, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.Conflict, "conflict", details)
    {
    }
}

public class ServerErrorException : HetznerCloudException
{
    public ServerErrorException(string message, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.InternalServerError, "internal_error", details)
    {
    }
}

public class ServiceUnavailableException : HetznerCloudException
{
    public TimeSpan? RetryAfter { get; }

    public ServiceUnavailableException(string message, TimeSpan? retryAfter = null, IEnumerable<ErrorDetail>? details = null)
        : base(message, HttpStatusCode.ServiceUnavailable, "service_unavailable", details)
    {
        RetryAfter = retryAfter;
    }
}