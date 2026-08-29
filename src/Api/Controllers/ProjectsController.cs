using Microsoft.AspNetCore.Mvc;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;

namespace DeveloperKnowledgeGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResponse<ProjectSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ProjectSummaryDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        return Ok(await _service.GetProjectsAsync(search, status, page, pageSize, ct));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ProjectDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectAsync(id, ct));
    }

    [HttpGet("{id}/dependencies")]
    [ProducesResponseType<IReadOnlyList<ProjectDependencyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectDependencyDto>>> GetDependencies(
        string id, [FromQuery] int? maxDepth, CancellationToken ct)
    {
        return Ok(await _service.GetProjectDependenciesAsync(id, maxDepth, ct));
    }

    [HttpGet("{id}/technologies")]
    [ProducesResponseType<IReadOnlyList<ProjectTechnologyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectTechnologyDto>>> GetTechnologies(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectTechnologiesAsync(id, ct));
    }

    [HttpGet("{id}/developers")]
    [ProducesResponseType<IReadOnlyList<ProjectDeveloperDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectDeveloperDto>>> GetDevelopers(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectDevelopersAsync(id, ct));
    }

    [HttpGet("{id}/repositories")]
    [ProducesResponseType<IReadOnlyList<ProjectRepositoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectRepositoryDto>>> GetRepositories(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectRepositoriesAsync(id, ct));
    }

    [HttpGet("{id}/tasks")]
    [ProducesResponseType<IReadOnlyList<ProjectTaskDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectTaskDto>>> GetTasks(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectTasksAsync(id, ct));
    }

    [HttpGet("{id}/contributors")]
    [ProducesResponseType<IReadOnlyList<ProjectContributorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectContributorDto>>> GetContributors(string id, CancellationToken ct)
    {
        return Ok(await _service.GetProjectContributorsAsync(id, ct));
    }

    [HttpGet("{id}/recommended-developers")]
    [ProducesResponseType<IReadOnlyList<RecommendedDeveloperDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecommendedDeveloperDto>>> GetRecommendedDevelopers(string id, CancellationToken ct)
    {
        return Ok(await _service.GetRecommendedDevelopersAsync(id, ct));
    }

    [HttpGet("{id}/indirect-technologies")]
    [ProducesResponseType<IReadOnlyList<IndirectTechnologyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<IndirectTechnologyDto>>> GetIndirectTechnologies(
        string id, [FromQuery] int? maxDepth, CancellationToken ct)
    {
        return Ok(await _service.GetProjectIndirectTechnologiesAsync(id, maxDepth, ct));
    }
}