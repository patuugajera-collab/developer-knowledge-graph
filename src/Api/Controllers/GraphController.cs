using Microsoft.AspNetCore.Mvc;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;

namespace DeveloperKnowledgeGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GraphController : ControllerBase
{
    private readonly IGraphService _service;

    public GraphController(IGraphService service)
    {
        _service = service;
    }

    [HttpGet("explore")]
    [ProducesResponseType<GraphResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GraphResponseDto>> Explore(
        [FromQuery] string entityType,
        [FromQuery] string id,
        [FromQuery] int? maxDepth,
        CancellationToken ct)
    {
        return Ok(await _service.GetGraphAsync(entityType, id, maxDepth, ct));
    }

    [HttpGet("shortest-path")]
    [ProducesResponseType<ShortestPathDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShortestPathDto>> ShortestPath(
        [FromQuery] string developerId,
        [FromQuery] string projectId,
        CancellationToken ct)
    {
        return Ok(await _service.GetShortestPathAsync(developerId, projectId, ct));
    }

    [HttpGet("central-technologies")]
    [ProducesResponseType<IReadOnlyList<CentralTechnologyDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CentralTechnologyDto>>> CentralTechnologies(
        [FromQuery] int? limit, CancellationToken ct)
    {
        var safeLimit = Math.Clamp(limit ?? 8, 1, 20);
        return Ok(await _service.GetCentralTechnologiesAsync(safeLimit, ct));
    }
}