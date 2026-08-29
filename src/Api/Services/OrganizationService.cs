using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Exceptions;
using DeveloperKnowledgeGraph.Api.Repositories;

namespace DeveloperKnowledgeGraph.Api.Services;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct);

    Task<IReadOnlyList<DeveloperSummaryDto>> GetOrganizationDevelopersAsync(string id, CancellationToken ct);
}

public sealed class OrganizationService : IOrganizationService
{
    private readonly IGraphRepository _repository;

    public OrganizationService(IGraphRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OrganizationSummaryDto>> GetOrganizationsAsync(CancellationToken ct)
    {
        return await _repository.GetOrganizationsAsync(ct);
    }

    public async Task<IReadOnlyList<DeveloperSummaryDto>> GetOrganizationDevelopersAsync(string id, CancellationToken ct)
    {
        if (!await _repository.NodeExistsAsync("Organization", id, ct))
        {
            throw new EntityNotFoundException("Organization", id);
        }

        return await _repository.GetOrganizationDevelopersAsync(id, ct);
    }
}