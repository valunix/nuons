namespace Nuons.Core.Tests;

public record NuonTestContext(string InputSourcePath, Type[] AssemblyMarkers, string? ReferencesSourcePath = null);
