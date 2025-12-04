using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class CombinedRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((c, _) => c.AssemblyName);

		var transientExists = GetProviderFor(context, typeof(TransientAttribute));
		var scopedExists = GetProviderFor(context, typeof(ScopedAttribute));
		var singletonExists = GetProviderFor(context, typeof(SingletonAttribute));
		var optionsExists = GetProviderFor(context, typeof(OptionsAttribute));

		var combinedProvider = assemblyNameProvider.Combine(transientExists)
			.Select((combined, _) => new CombinedRegistrationsIncrement() { AssemblyName = combined.Left, Transient = combined.Right })
			.Combine(scopedExists)
			.Select((combined, _) => combined.Left with { Scoped = combined.Right })
			.Combine(singletonExists)
			.Select((combined, _) => combined.Left with { Singleton = combined.Right })
			.Combine(optionsExists)
			.Select((combined, _) => combined.Left with { Options = combined.Right });

		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private IncrementalValueProvider<bool> GetProviderFor(IncrementalGeneratorInitializationContext context, Type type)
	{
		var attributeExists = context.SyntaxProvider.ForAttributeWithMetadataName(
			type.FullName,
			Syntax.IsClassNode,
			(_, __) => true
		)
			.Collect()
			.Select((collected, _) => collected.Any());
		return attributeExists;
	}

	private void GenerateSources(
		SourceProductionContext context,
		CombinedRegistrationsIncrement increment)
	{
		if (increment.AssemblyName is null || !increment.HasRegistrations)
		{
			return;
		}

		var sourceBuilder = new CombinedRegistrationSourceBuilder(increment);
		var source = sourceBuilder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		var className = DependancyInjectionSources.GetCombinedRegistrationClassName(increment.AssemblyName);
		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}
