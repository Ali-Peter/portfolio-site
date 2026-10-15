using System.Reflection;

namespace Portfolio.Application;

/// <summary>
/// Anchor type for assembly-level operations (DI scanning, architecture tests).
/// </summary>
public static class AssemblyMarker
{
    public static Assembly Assembly => typeof(AssemblyMarker).Assembly;
}
