using Nuons.CodeInjection.Abstractions;
using Nuons.DependencyInjection.Abstractions;
using Nuons.EndToEnd.Api;

[Transient]
[InjectConstructor]
public partial class GlobalNamespaceService
{
	public const string Value = "GlobalNamespaceServiceValue";

	[Injected]
	private readonly IEmptyConstructorService emptyConstructorService;

	public string GetValue() => Value;
}
