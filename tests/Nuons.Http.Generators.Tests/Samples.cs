using Nuons.Http.Abstractions;

namespace Nuons.Http.Generators.Tests;

[Route]
internal partial class EmptyHandler
{
	[Get]
	public void GetEmpty() { }
}

[Route("/complex/")]
internal partial class ComplexHandler
{
	[Get("/sample/{id}/")]
	public void Get(Guid id, string name, GenericClass<string> generic) { }
}

internal class GenericClass<T>;

[Route("/domain-object")]
internal partial class DomainObjectHandler
{
	[Get("{id}")]
	public void GetDomainObject(string id) { }

	[Post]
	public void CreateDomainObject(DomainObject domainObject) { }
}

internal class DomainObject;
