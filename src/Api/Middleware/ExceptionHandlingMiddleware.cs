using System.Net;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using Microsoft.Data.SqlClient;

namespace DeveloperKnowledgeGraph.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled by the client.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, detail, logLevel) = Categorize(exception);

        if (logLevel == LogLevel.Error)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Path}.", context.Request.Path);
            _logger.LogWarning("Request trace id {TraceId} returned status {StatusCode}.", context.TraceIdentifier, statusCode);
        }
        else
        {
            _logger.LogWarning("Request trace id {TraceId} returned status {StatusCode} with message {Message}.",
                context.TraceIdentifier, statusCode, message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new ErrorResponse(message, detail, context.TraceIdentifier);
        await context.Response.WriteAsJsonAsync(payload, context.RequestAborted);
    }

    private static (int StatusCode, string Message, string? Detail, LogLevel Level) Categorize(Exception exception)
    {
        return exception switch
        {
            EntityNotFoundException ex => (
                (int)HttpStatusCode.NotFound,
                ex.Message,
                null,
                LogLevel.Information),

            ValidationException ex => (
                (int)HttpStatusCode.BadRequest,
                ex.Message,
                null,
                LogLevel.Information),

            DatabaseUnavailableException ex => (
                (int)HttpStatusCode.ServiceUnavailable,
                "Unable to connect to the database. Please try again.",
                SafeDetail(ex.InnerException),
                LogLevel.Warning),

            SqlException ex => (
                (int)HttpStatusCode.ServiceUnavailable,
                "Unable to connect to the database. Please try again.",
                SafeDetail(ex.InnerException),
                LogLevel.Warning),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                null,
                LogLevel.Error),
        };
    }

    private static string? SafeDetail(Exception? exception)
    {
        if (exception is null)
        {
            return null;
        }

        var message = exception.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        return message.Length > 300 ? message[..300] : message;
    }
}