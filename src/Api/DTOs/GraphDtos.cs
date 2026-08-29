namespace DeveloperKnowledgeGraph.Api.DTOs;

public sealed record GraphNodeDto(
    string Id,
    string Label,
    string Type,
    IReadOnlyDictionary<string, object?> Properties);

public sealed record GraphEdgeDto(
    string Id,
    string Source,
    string Target,
    string Type,
    IReadOnlyDictionary<string, object?> Properties);

public sealed record GraphResponseDto(
    IReadOnlyList<GraphNodeDto> Nodes,
    IReadOnlyList<GraphEdgeDto> Edges,
    string RootId,
    int MaxDepth);

public sealed record PathStepDto(
    string NodeType,
    string NodeId,
    string NodeName,
    string? Relationship);

public sealed record ShortestPathDto(
    string DeveloperId,
    string DeveloperName,
    string ProjectId,
    string ProjectName,
    IReadOnlyList<PathStepDto> Steps,
    int Length);