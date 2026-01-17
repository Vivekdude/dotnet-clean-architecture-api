using System.Net;
using System.Text.Json;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Domain.Exceptions;

namespace CleanArchitecture.API.Middleware;

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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = notFoundException.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", notFoundException.Message);
                break;

            case ConflictException conflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = conflictException.Message;
                _logger.LogWarning(exception, "Conflict occurred: {Message}", conflictException.Message);
                break;

            case Domain.Exceptions.ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = "Validation failed";
                errorResponse.Errors = validationException.Errors.SelectMany(e => e.Value).ToList();
                _logger.LogWarning(exception, "Validation failed: {Message}", validationException.Message);
                break;

            case DomainException domainException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = domainException.Message;
                _logger.LogWarning(exception, "Domain error: {Message}", domainException.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = "An unexpected error occurred";
#if DEBUG
                errorResponse.Details = exception.Message;
                errorResponse.StackTrace = exception.StackTrace;
#endif
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
}
