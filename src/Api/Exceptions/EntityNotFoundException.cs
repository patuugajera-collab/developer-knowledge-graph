namespace DeveloperKnowledgeGraph.Api.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityType, string id)
        : base($"The {entityType} with id '{id}' was not found in the graph.")
    {
        EntityType = entityType;
        Id = id;
    }

    public string EntityType { get; }
    public string Id { get; }
}