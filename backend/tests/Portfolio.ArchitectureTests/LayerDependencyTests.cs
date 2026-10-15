using System.Reflection;
using Portfolio.Application;
using Portfolio.Domain.Entities;

namespace Portfolio.ArchitectureTests;

/// <summary>
/// Enforces the dependency rule from ADR-0001:
/// Domain references nothing; Application references only Domain.
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_DoesNotReferenceAnyOtherPortfolioLayer()
    {
        var referenced = typeof(BaseEntity).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referenced, IsPortfolioAssembly);
    }

    [Fact]
    public void Application_NeverReferencesOuterLayers()
    {
        var portfolioReferences = typeof(AssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Where(IsPortfolioAssembly)
            .Select(a => a.Name!)
            .ToArray();

        // Allowed: Portfolio.Domain only. Api and Infrastructure are forbidden.
        // (Domain only appears in this list once Application code actually uses
        // domain types — an unused project reference is not emitted by Roslyn.)
        Assert.DoesNotContain(
            portfolioReferences,
            name => name is "Portfolio.Api" or "Portfolio.Infrastructure");
    }

    private static bool IsPortfolioAssembly(AssemblyName assembly) =>
        assembly.Name?.StartsWith("Portfolio.", StringComparison.Ordinal) == true;
}
