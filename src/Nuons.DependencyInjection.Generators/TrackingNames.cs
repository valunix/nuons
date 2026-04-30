namespace Nuons.DependencyInjection.Generators;

internal static class TrackingNames
{
	public const string AssemblyName = nameof(AssemblyName);
	public const string TransientProvider = nameof(TransientProvider);
	public const string ScopedProvider = nameof(ScopedProvider);
	public const string SingletonProvider = nameof(SingletonProvider);
	public const string AllRegistrations = nameof(AllRegistrations);
	public const string IncrementProvider = nameof(IncrementProvider);
	public const string OptionsProvider = nameof(OptionsProvider);
	public const string OptionsIncrementProvider = nameof(OptionsIncrementProvider);

	private const string ServiceProviderPrefix = "ServiceProvider_";

	public static string ServiceProviderNonGeneric(Lifetime lifetime) => $"{ServiceProviderPrefix}{lifetime.ToMethodName()}_NonGeneric";
	public static string ServiceProviderGeneric(Lifetime lifetime) => $"{ServiceProviderPrefix}{lifetime.ToMethodName()}_Generic";
}
