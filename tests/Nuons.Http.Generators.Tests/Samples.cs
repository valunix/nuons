using Nuons.Http.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

[Route("/domain-object")]
internal partial class GetDomainObjectHandler
{

	[Get]
	public void GetSimple(Guid id) { }

	[Get]
	public void GetDomainObject(Guid id, string name, CustomClass<string> custom) { }
}

internal class CustomClass<T>;
