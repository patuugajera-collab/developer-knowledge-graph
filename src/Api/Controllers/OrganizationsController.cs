using Microsoft.AspNetCore.Mvc;
using DeveloperKnowledgeGraph.Api.DTOs;
using DeveloperKnowledgeGraph.Api.Services;

namespace DeveloperKnowledgeGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<OrganizationSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrganizationSummaryDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetOrganizationsAsync(ct));
    }

    [HttpGet("{id}/developers")]
    [ProducesResponseType<IReadOnlyList<DeveloperSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DeveloperSummaryDto>>> GetDevelopers(string id, CancellationToken ct)
    {
        return Ok(await _service.GetOrganizationDevelopersAsync(id, ct));
    }
}