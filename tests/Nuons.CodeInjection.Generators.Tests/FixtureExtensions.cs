using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using Nuons.CodeInjection.Abstractions;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests;

internal static class FixtureExtensions
{
	private const string SamplesPath = "../../../Samples.cs";
	private static readonly Type[] AssemblyMarkers = [typeof(CodeInjectionAbstractionsAssemblyMarker), typeof(Options)];

	private static readonly NuonGeneratorTestContext Context = new(SamplesPath, AssemblyMarkers);

	public static string GenerateSources<TGenerator>(this NuonGeneratorFixture fixture)
		where TGenerator : IIncrementalGenerator, new()
	{
		return fixture.GenerateSources<TGenerator>(Context);
	}

	public static void RunGenerator<TGenerator>(this NuonGeneratorFixture fixture, ITestOutputHelper output)
		where TGenerator : IIncrementalGenerator, new()
	{
		fixture.RunGenerator<TGenerator>(Context, output);
	}

	private const string MappingSamplesSharedPath = "../../../MappingSamplesShared.cs";
	private const string MappingSamplesDir = "../../../MappingSamples";

	private static NuonGeneratorTestContext MappingContext(string sampleFileName) =>
		new($"{MappingSamplesDir}/{sampleFileName}", AssemblyMarkers, MappingSamplesSharedPath);

	public static string GenerateMappingSources(this NuonGeneratorFixture fixture, string sampleFileName) =>
		fixture.GenerateSources<DtoMappingGenerator>(MappingContext(sampleFileName));

	public static void RunMappingGenerator(this NuonGeneratorFixture fixture, string sampleFileName, ITestOutputHelper output) =>
		fixture.RunGenerator<DtoMappingGenerator>(MappingContext(sampleFileName), output);
}
