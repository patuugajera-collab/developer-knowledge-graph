using DeveloperKnowledgeGraph.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeveloperKnowledgeGraph.Api.Data;

/// <summary>
/// EF Core model for the Developer Knowledge &amp; Dependency Graph.
/// Node types map to tables; each relationship type is a dedicated
/// join-table carrying its edge properties.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Developer> Developers => Set<Developer>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    public DbSet<WorksForEdge> WorksForRelations => Set<WorksForEdge>();
    public DbSet<OwnsEdge> OwnsRelations => Set<OwnsEdge>();
    public DbSet<WorksOnEdge> WorksOnRelations => Set<WorksOnEdge>();
    public DbSet<UsesEdge> UsesRelations => Set<UsesEdge>();
    public DbSet<DependsOnEdge> DependsOnRelations => Set<DependsOnEdge>();
    public DbSet<HasSkillEdge> HasSkillRelations => Set<HasSkillEdge>();
    public DbSet<ContributedToEdge> ContributedToRelations => Set<ContributedToEdge>();
    public DbSet<RequiresSkillEdge> RequiresSkillRelations => Set<RequiresSkillEdge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("Organizations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Developer>(e =>
        {
            e.ToTable("Developers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("Projects");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Technology>(e =>
        {
            e.ToTable("Technologies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Repository>(e =>
        {
            e.ToTable("Repositories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<WorkTask>(e =>
        {
            e.ToTable("Tasks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired();
            e.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<WorksForEdge>(e =>
        {
            e.ToTable("WorksFor");
            e.HasKey(x => new { x.DeveloperId, x.OrganizationId });
        });

        modelBuilder.Entity<OwnsEdge>(e =>
        {
            e.ToTable("Owns");
            e.HasKey(x => new { x.OrganizationId, x.ProjectId });
        });

        modelBuilder.Entity<WorksOnEdge>(e =>
        {
            e.ToTable("WorksOn");
            e.HasKey(x => new { x.DeveloperId, x.ProjectId });
            e.HasIndex(x => x.ProjectId);
            e.HasIndex(x => x.DeveloperId);
        });

        modelBuilder.Entity<UsesEdge>(e =>
        {
            e.ToTable("Uses");
            e.HasKey(x => new { x.ProjectId, x.TechnologyId });
            e.HasIndex(x => x.TechnologyId);
        });

        modelBuilder.Entity<DependsOnEdge>(e =>
        {
            e.ToTable("DependsOn");
            e.HasKey(x => new { x.ProjectId, x.DependencyProjectId });
        });

        modelBuilder.Entity<HasSkillEdge>(e =>
        {
            e.ToTable("HasSkill");
            e.HasKey(x => new { x.DeveloperId, x.TechnologyId });
            e.HasIndex(x => x.TechnologyId);
        });

        modelBuilder.Entity<ContributedToEdge>(e =>
        {
            e.ToTable("ContributedTo");
            e.HasKey(x => new { x.DeveloperId, x.RepositoryId });
        });

        modelBuilder.Entity<RequiresSkillEdge>(e =>
        {
            e.ToTable("RequiresSkill");
            e.HasKey(x => new { x.TaskId, x.TechnologyId });
            e.HasIndex(x => x.TechnologyId);
        });
    }
}