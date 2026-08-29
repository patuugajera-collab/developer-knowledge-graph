using System.Text;
using Microsoft.AspNetCore.Http;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Middleware;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace DeveloperKnowledgeGraph.Tests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_EntityNotFound_MapsTo404()
    {
        var (status, body) = await RunAsync(new EntityNotFoundException("Developer", "nope"));
        status.Should().Be(StatusCodes.Status404NotFound);
        body!.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_MapsTo400()
    {
        var (status, _) = await RunAsync(new ValidationException("maxDepth out of range"));
        status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_DatabaseUnavailable_MapsTo503_WithSafeMessage()
    {
        var (status, body) = await RunAsync(new DatabaseUnavailableException("boom"));
        status.Should().Be(StatusCodes.Status503ServiceUnavailable);
        body!.Message.Should().Be("Unable to connect to the database. Please try again.");
        body.Message.Should().NotContain("boom");
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_MapsTo500_WithoutStackTrace()
    {
        var (status, body) = await RunAsync(new InvalidOperationException("secret detail"));
        status.Should().Be(StatusCodes.Status500InternalServerError);
        body!.Message.Should().Be("An unexpected error occurred. Please try again later.");
        body.Detail.Should().BeNull();
    }

    private static async Task<(int Status, ErrorResponse? Body)> RunAsync(Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-123";
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await middleware.InvokeAsync(context);

        responseBody.Position = 0;
        var json = new StreamReader(responseBody, Encoding.UTF8).ReadToEnd();
        var body = json.Length == 0
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return (context.Response.StatusCode, body);
    }
}