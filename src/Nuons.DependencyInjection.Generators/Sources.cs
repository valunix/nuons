using System.Text.RegularExpressions;

namespace Nuons.DependencyInjection.Generators;

internal static class Sources
{
	private const string GeneratedExtension = ".g.cs";

	public static string GeneratedNameHint(string name) => $"{name}{GeneratedExtension}";

	public const string Tab1 = "\t";
	public const string Tab2 = "\t\t";
	public const string NewLine = "\n";

	public static string ToMethodName(this Lifetime lifetime) =>
		lifetime switch
		{
			Lifetime.Singleton => "Singleton",
			Lifetime.Scoped => "Scoped",
			Lifetime.Transient => "Transient",
			_ => throw new ArgumentOutOfRangeException(nameof(lifetime)),
		};

	public static string TrimForClassName(string assemblyName) =>
		Regex.Replace(assemblyName, @"[^a-zA-Z0-9]", string.Empty);

	public static string GetLifetimeRegistrationClassName(string assemblyName, Lifetime lifetime) =>
		$"{lifetime}Registration{TrimForClassName(assemblyName)}";

	public static string GetServiceRegistrationClassName(string assemblyName) =>
		$"ServiceRegistration{TrimForClassName(assemblyName)}";

	public static string GetCombinedRegistrationClassName(string assemblyName) =>
		$"CombinedRegistration{TrimForClassName(assemblyName)}";

	public static string GetOptionsRegistrationClassName(string assemblyName) =>
		$"OptionsRegistration{TrimForClassName(assemblyName)}";
}
