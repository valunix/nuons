namespace Nuons.Core.Tests;

public record NuonSourceIncrement(string Source, string? ReferencesSource = null)
{
	public static NuonSourceIncrement FromFile(string path, string? referencesPath = null)
		=> new(File.ReadAllText(path), referencesPath is not null ? File.ReadAllText(referencesPath) : null);
}
