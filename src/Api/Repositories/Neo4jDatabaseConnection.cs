using Neo4j.Driver;

namespace DeveloperKnowledgeGraph.Api.Repositories;

/// <summary>
/// Lightweight abstraction over the backing database used for health checks.
/// </summary>
public interface IDatabaseConnection
{
	Task<bool> IsHealthyAsync(CancellationToken ct = default);
}

/// <summary>
/// CognoDB-backed health check: verifies the configured instance by running a
/// lightweight Cypher query over the Bolt connection.
/// </summary>
public sealed class CognoDbConnection : IDatabaseConnection
{
	private readonly IDriver _driver;

	public CognoDbConnection(IDriver driver)
	{
		_driver = driver;
	}

	public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
	{
		try
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("RETURN 1");
			foreach (var record in await cursor.ToListAsync(cancellationToken: ct))
			{
				_ = record;
			}

			return true;
		}
		catch
		{
			return false;
		}
	}
}
