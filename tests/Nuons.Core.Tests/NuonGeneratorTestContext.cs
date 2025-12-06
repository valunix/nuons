namespace Nuons.Core.Tests;

public record NuonGeneratorTestContext(string InputSourcePath, Type[] AssemblyMarkers, string? ReferencesSourcePath = null);
