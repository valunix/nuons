using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Configuration;

[Generator]
internal class OptionsRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((compilation, _) => compilation.AssemblyName);

		var optionsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(OptionsAttribute).FullName,
			Syntax.IsClassNode,
			ExtractOptionDefinitions
		)
			.Where(optionDefinition => optionDefinition is not null)
			.Collect();

		var optionsIncrementProvider = assemblyNameProvider.Combine(optionsProvider)
			.Select((combined, _) => new OptionsRegistrationIncrement(combined.Left, combined.Right));

		context.RegisterSourceOutput(optionsIncrementProvider, GenerateSources);
	}

	private OptionsRegistration? ExtractOptionDefinitions(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<OptionsAttribute>();
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 1)
		{
			return null;
		}

		if (constructorArguments[0].Value is not string sectionArg)
		{
			return null;
		}

		return new OptionsRegistration(sectionArg, symbol.ToFullName());
	}

	private void GenerateSources(SourceProductionContext context, OptionsRegistrationIncrement increment)
	{
		if (increment.AssemblyName is null || !increment.Registrations.Any())
		{
			return;
		}

		var className = Sources.GetOptionsRegistrationClassName(increment.AssemblyName);
		var builder = new OptionsRegistrationSourceBuilder(className);
		foreach (var registration in increment.Registrations)
		{
			if (registration is null)
			{
				continue;
			}

			builder.WithOptions(registration);
		}

		var sourceText = SourceText.From(builder.Build(), Encoding.UTF8);
		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}
