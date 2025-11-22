using Nuons.Http.Abstractions;

[assembly: Nuons.Core.Abstractions.AssemblyHasNuons]

namespace Nuons.Http.Generators.Tests;

[Route("/sub-domain-object")]
internal partial class SubDomainObjectHandler
{
	[Get]
	public void GetSubDomainObject() { }

	[Post]
	public void CreateSubDomainObject(SubDomainObject subDomainObject) { }
}

internal class SubDomainObject;
