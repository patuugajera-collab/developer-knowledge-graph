using Microsoft.AspNetCore.Mvc;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;

namespace DeveloperKnowledgeGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DevelopersController : ControllerBase
{
    private readonly IDeveloperService _service;

    public DevelopersController(IDeveloperService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResponse<DeveloperSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<DeveloperSummaryDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        return Ok(await _service.GetDevelopersAsync(search, page, pageSize, ct));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<DeveloperDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeveloperDetailDto>> GetById(string id, CancellationToken ct)
    {
        return Ok(await _service.GetDeveloperAsync(id, ct));
    }

    [HttpGet("{id}/projects")]
    [ProducesResponseType<IReadOnlyList<DeveloperProjectDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DeveloperProjectDto>>> GetProjects(string id, CancellationToken ct)
    {
        return Ok(await _service.GetDeveloperProjectsAsync(id, ct));
    }

    [HttpGet("{id}/skills")]
    [ProducesResponseType<IReadOnlyList<DeveloperSkillDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DeveloperSkillDto>>> GetSkills(string id, CancellationToken ct)
    {
        return Ok(await _service.GetDeveloperSkillsAsync(id, ct));
    }

    [HttpGet("{id}/repositories")]
    [ProducesResponseType<IReadOnlyList<DeveloperRepositoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DeveloperRepositoryDto>>> GetRepositories(string id, CancellationToken ct)
    {
        return Ok(await _service.GetDeveloperRepositoriesAsync(id, ct));
    }
}