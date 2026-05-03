using System;
using Nuons.Http.Abstractions;

// TODO: Manual application is required because NuonGeneratorFixture compiles this file
// in-memory and AssemblyMarkerGenerator does not run on that reference compilation.
// Consider extending NuonGeneratorFixture to inject the marker so this can be removed.
[assembly: Nuons.Core.Abstractions.AssemblyHasNuons]

namespace Nuons.Http.Generators.Tests;

[Route("/sub-domain-object")]
internal partial class SubDomainObjectHandler
{
	[Get]
	public void Get() { }

	[Post]
	public void Create(SubDomainObject subDomainObject) { }

	[Delete("{id}")]
	public void Delete(Guid id) { }

	[Put("{id}")]
	public void Update(int id, SubDomainObject subDomainObject) { }
}

internal class SubDomainObject;
