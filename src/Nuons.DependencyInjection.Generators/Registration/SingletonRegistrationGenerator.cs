using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class SingletonRegistrationGenerator : RegistrationGenerator<SingletonAttribute>
{
	public SingletonRegistrationGenerator() : base(Lifetime.Singleton) { }
}
