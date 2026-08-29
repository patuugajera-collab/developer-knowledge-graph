using DeveloperKnowledgeGraph.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DeveloperKnowledgeGraph.Api.Repositories;

/// <summary>
/// Lightweight abstraction over the backing database used for health checks.
/// </summary>
public interface IDatabaseConnection
{
    Task<bool> IsHealthyAsync(CancellationToken ct = default);
}

/// <summary>
/// SQL-backed health check: verifies the configured connection by opening a
/// connection to the database.
/// </summary>
public sealed class EFDatabaseConnection : IDatabaseConnection
{
    private readonly AppDbContext _db;

    public EFDatabaseConnection(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.Database.CanConnectAsync(ct);
        }
        catch
        {
            return false;
        }
    }
}