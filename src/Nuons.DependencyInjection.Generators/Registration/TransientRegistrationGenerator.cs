using Microsoft.CodeAnalysis;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class TransientRegistrationGenerator : RegistrationGenerator<TransientAttribute>
{
	public TransientRegistrationGenerator() : base(Lifetime.Transient) { }
}
