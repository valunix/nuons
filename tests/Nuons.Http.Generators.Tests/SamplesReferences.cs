using Nuons.Http.Abstractions;

[assembly: Nuons.Core.Abstractions.AssemblyHasNuons]

namespace Nuons.DependencyInjection.Generators.Tests;

[Route("/sub-domain-object")]
internal partial class GetSubDomainObjectHandler
{
	[Get]
	public void GetSubDomainObject() { }
}
