using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Tests;

/// <summary>
/// Host factory that lets tests substitute the database connection and repository.
/// </summary>
public sealed class WebTestFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _overrides;

    public WebTestFactory(Action<IServiceCollection>? overrides = null)
    {
        _overrides = overrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        if (_overrides is not null)
        {
            builder.ConfigureServices(_overrides);
        }
    }
}

internal sealed class StubConnection : IDatabaseConnection
{
    private readonly bool _available;

    public StubConnection(bool available)
    {
        _available = available;
    }

    public Task<bool> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult(_available);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceWith<TService>(this IServiceCollection services, TService instance)
        where TService : class
    {
        services.RemoveAll(typeof(TService));
        services.AddSingleton(instance);
        return services;
    }
}