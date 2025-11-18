using System.Text.RegularExpressions;

namespace Nuons.Core.Generators;

public static class Sources
{
	private const string GeneratedExtension = ".g.cs";

	public static string GeneratedNameHint(string name) => $"{name}{GeneratedExtension}";

	public const string Tab1 = "\t";
	public const string Tab2 = "\t\t";
	public const string NewLine = "\n";

	public static string TrimForClassName(string assemblyName) =>
		Regex.Replace(assemblyName, @"[^a-zA-Z0-9]", string.Empty);
}
