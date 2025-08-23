using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class ScopedRegistrationGenerator : RegistrationGenerator<ScopedAttribute>
{
	public ScopedRegistrationGenerator() : base(Lifetime.Scoped) { }
}
