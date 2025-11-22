using Nuons.Http.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

[Route("/domain-object")]
internal partial class GetDomainObjectHandler
{
	[Get("{id}")]
	public void GetSimple(string id) { }

	[Get]
	public void GetDomainObject(Guid id, string name, CustomClass<string> custom) { }
}

internal class CustomClass<T>;

[Route]
internal partial class EmptyHandler
{
	[Get]
	public void GetSimple() { }
}
