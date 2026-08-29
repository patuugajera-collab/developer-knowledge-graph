using Microsoft.Extensions.Options;
using DeveloperKnowledgeGraph.Api.Configuration;

namespace DeveloperKnowledgeGraph.Api.Configuration;

public static class AppConfig
{
    public static CorsOptions BindCorsOptions(WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(CorsOptions.SectionName);
        var options = new CorsOptions();
        section.Bind(options);

        options.AllowedOrigins = FirstNonEmpty(
            Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS"),
            options.AllowedOrigins);

        builder.Services.AddSingleton(Options.Create(options));
        return options;
    }

    private static string FirstNonEmpty(string? candidate, string fallback)
    {
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate.Trim();
    }
}