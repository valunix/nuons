using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators;

internal static class DependancyInjectionSources
{
	public static string ToMethodName(this Lifetime lifetime) =>
		lifetime switch
		{
			Lifetime.Singleton => "Singleton",
			Lifetime.Scoped => "Scoped",
			Lifetime.Transient => "Transient",
			_ => throw new ArgumentOutOfRangeException(nameof(lifetime)),
		};

	public static string GetLifetimeRegistrationClassName(string assemblyName, Lifetime lifetime) =>
		$"{lifetime}Registration{Sources.TrimForClassName(assemblyName)}";

	public static string GetServiceRegistrationClassName(string assemblyName) =>
		$"ServiceRegistration{Sources.TrimForClassName(assemblyName)}";

	public static string GetCombinedRegistrationClassName(string assemblyName) =>
		$"CombinedRegistration{Sources.TrimForClassName(assemblyName)}";

	public static string GetOptionsRegistrationClassName(string assemblyName) =>
		$"OptionsRegistration{Sources.TrimForClassName(assemblyName)}";
}
