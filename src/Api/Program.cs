using DeveloperKnowledgeGraph.Api.Configuration;
using DeveloperKnowledgeGraph.Api.Data;
using DeveloperKnowledgeGraph.Api.Middleware;
using DeveloperKnowledgeGraph.Api.Repositories;
using DeveloperKnowledgeGraph.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var environmentFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
EnvFileLoader.Load(environmentFile);
EnvFileLoader.LogLoadedVariables(environmentFile);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<IDatabaseConnection, EFDatabaseConnection>();
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
            "REST API for exploring a developer knowledge and dependency graph stored in SQL Server.",
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