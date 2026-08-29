using Microsoft.AspNetCore.Mvc;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;

namespace DeveloperKnowledgeGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TechnologiesController : ControllerBase
{
    private readonly ITechnologyService _service;

    public TechnologiesController(ITechnologyService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResponse<TechnologySummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<TechnologySummaryDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        return Ok(await _service.GetTechnologiesAsync(search, category, page, pageSize, ct));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<TechnologyDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnologyDetailDto>> GetById(string id, CancellationToken ct)
    {
        return Ok(await _service.GetTechnologyAsync(id, ct));
    }

    [HttpGet("{id}/developers")]
    [ProducesResponseType<IReadOnlyList<TechnologyDeveloperDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TechnologyDeveloperDto>>> GetDevelopers(string id, CancellationToken ct)
    {
        return Ok(await _service.GetTechnologyDevelopersAsync(id, ct));
    }

    [HttpGet("{id}/projects")]
    [ProducesResponseType<IReadOnlyList<TechnologyProjectDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TechnologyProjectDto>>> GetProjects(string id, CancellationToken ct)
    {
        return Ok(await _service.GetTechnologyProjectsAsync(id, ct));
    }

    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken ct)
    {
        return Ok(await _service.GetTechnologyCategoriesAsync(ct));
    }
}