using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Nuons.Core.Abstractions;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators.Startup;

[Generator]
internal partial class CombinedServiceRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var referencesProvider = context.CompilationProvider
			.SelectMany(ExtractIncrements)
			.Collect();

		context.RegisterSourceOutput(referencesProvider, GenerateSources);
	}

	private static ImmutableArray<CombinedServiceRegistrationIncrement> ExtractIncrements(Compilation compilation, CancellationToken cancellationToken)
	{
		var nuonMarkerAttribute = compilation.GetTypeByMetadataName(typeof(AssemblyHasNuonsAttribute).FullName);
		if (nuonMarkerAttribute is null)
		{
			return [];
		}

		var results = compilation.SourceModule.ReferencedAssemblySymbols
			.Where(assembly => assembly.HasAttribute(nuonMarkerAttribute))
			.Concat([compilation.Assembly])
			.Select(assembly => new CombinedServiceRegistrationIncrement(assembly.Name));

		return [.. results];
	}

	private static void GenerateSources(SourceProductionContext context, ImmutableArray<CombinedServiceRegistrationIncrement> increments)
	{
		var className = "NuonDependencyInjectionExtensions";
		var builder = new CombinedServiceRegistrationSourceBuilder(className);
		var source = builder.Build(increments);
		context.AddSource(Sources.GeneratedNameHint(className), source);
	}
}
