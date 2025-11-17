using Microsoft.CodeAnalysis;
using Nuons.Core.Tests;
using Nuons.Http.Abstractions;

namespace Nuons.DependencyInjection.Generators.Tests;

internal static class FixtureExtensions
{
	private const string SamplesPath = "../../../Samples.cs";
	private static readonly Type[] AssemblyMarkers = [typeof(HttpAbstractionsAssemblyMarker)];

	private static readonly NuonTestContext Context = new NuonTestContext(SamplesPath, AssemblyMarkers);

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
}
