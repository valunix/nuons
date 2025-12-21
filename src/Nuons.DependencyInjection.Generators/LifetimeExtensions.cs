namespace Nuons.DependencyInjection.Generators;

internal static class LifetimeExtensions
{
	public static string ToMethodName(this Lifetime lifetime) =>
		lifetime switch
		{
			Lifetime.Singleton => "Singleton",
			Lifetime.Scoped => "Scoped",
			Lifetime.Transient => "Transient",
			_ => throw new ArgumentOutOfRangeException(nameof(lifetime)),
		};
}
