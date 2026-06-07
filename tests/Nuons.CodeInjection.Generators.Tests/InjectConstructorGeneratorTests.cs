using Microsoft.Extensions.Options;
using Nuons.CodeInjection.Abstractions;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests.Injection;

public class InjectConstructorGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public void DevRunGenerator() => fixture.RunGenerator<InjectConstructorGenerator>(output);

	[Fact]
	public Task InjectedConstructorIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateSources<InjectConstructorGenerator>();
		return Verify(sources);
	}

	[Fact]
	public Task GlobalNamespaceClassIsGeneratedCorrectly()
	{
		const string source = @"using Nuons.CodeInjection.Abstractions;

public interface IGlobalDependency;

[InjectConstructor]
internal partial class GlobalScopedService
{
	[Injected] private readonly IGlobalDependency dependency;
}";
		var context = new NuonGeneratorTestContext(
			[new NuonSourceIncrement(source)],
			[typeof(CodeInjectionAbstractionsAssemblyMarker), typeof(Options)]);

		var sources = fixture.GenerateSources<InjectConstructorGenerator>(context);
		return Verify(sources);
	}

	[Fact]
	public Task EmptyConstructorIsGeneratedWhenNoInjectedFields()
	{
		const string source = @"using Nuons.CodeInjection.Abstractions;

namespace Sample;

[InjectConstructor]
internal partial class NoInjectedFieldsService
{
	private readonly int unrelated;
}";
		var context = new NuonGeneratorTestContext(
			[new NuonSourceIncrement(source)],
			[typeof(CodeInjectionAbstractionsAssemblyMarker), typeof(Options)]);

		var sources = fixture.GenerateSources<InjectConstructorGenerator>(context);
		return Verify(sources);
	}
}
