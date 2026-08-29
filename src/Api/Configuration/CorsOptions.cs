namespace DeveloperKnowledgeGraph.Api.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string AllowedOrigins { get; set; } = "http://localhost:4200";
}