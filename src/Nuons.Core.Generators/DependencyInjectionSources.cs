namespace Nuons.Core.Generators;

public static class DependencyInjectionSources
{
	public static string GetServiceRegistrationClassName(string assemblyName) =>
		$"ServiceRegistration{Sources.TrimForClassName(assemblyName)}";

	public static string GetOptionsRegistrationClassName(string assemblyName) =>
		$"OptionsRegistration{Sources.TrimForClassName(assemblyName)}";
}
