using Nuons.CodeInjection.Abstractions;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[Transient<IEmptyConstructorService>]
[InjectConstructor]
internal partial class EmptyConstructorService : IEmptyConstructorService
{
	public const string Value = "EmptyConstructorValue";

	public string GetValue() => Value;
}
