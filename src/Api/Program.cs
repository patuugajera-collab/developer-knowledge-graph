using DeveloperKnowledgeGraph.Api.Configuration;
using DeveloperKnowledgeGraph.Api.Middleware;
using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using Microsoft.OpenApi;
using Neo4j.Driver;

var environmentFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
EnvFileLoader.Load(environmentFile);
EnvFileLoader.LogLoadedVariables(environmentFile);

var builder = WebApplication.CreateBuilder(args);

var cognoUri = builder.Configuration.GetConnectionString("CognoDB")
    ?? throw new InvalidOperationException("Connection string 'CognoDB' is not configured.");
var cognoPassword = Environment.GetEnvironmentVariable("COGNODB_PASSWORD")
    ?? builder.Configuration["CognoDB:Password"]
    ?? throw new InvalidOperationException("CognoDB password is not configured (set COGNODB_PASSWORD).");

builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(cognoUri, AuthTokens.Basic("cognodb", cognoPassword)));

builder.Services.AddControllers();
builder.Services.AddScoped<IDatabaseConnection, CognoDbConnection>();
builder.Services.AddScoped<IGraphRepository, GraphRepository>();

builder.Services.AddScoped<IDeveloperService, DeveloperService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITechnologyService, TechnologyService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IHealthService, HealthService>();

var corsOptions = AppConfig.BindCorsOptions(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Developer Knowledge Graph API",
        Version = "v1",
        Description =
            "REST API for exploring a developer knowledge and dependency graph stored in CognoDB.",
    });
});

var origins = corsOptions.AllowedOrigins
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.MapControllers();

app.Run();

public partial class Program
{
}