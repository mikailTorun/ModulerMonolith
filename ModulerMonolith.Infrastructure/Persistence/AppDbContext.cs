using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ModulerMonolith.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private static readonly List<Assembly> _moduleAssemblies = [];

    public static void RegisterModuleAssembly(Assembly assembly)
    {
        if (!_moduleAssemblies.Contains(assembly))
            _moduleAssemblies.Add(assembly);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var assembly in _moduleAssemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
}
