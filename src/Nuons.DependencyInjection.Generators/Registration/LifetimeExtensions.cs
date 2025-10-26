namespace Nuons.DependencyInjection.Generators.Registration;

public static class LifetimeExtensions
{
	public static bool IsLifetime(this int lifetime) =>
		lifetime switch
		{
			>= 0 and <= 2 => true,
			_ => false,
		};
}
