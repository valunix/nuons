using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class RootRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var incrementProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
				typeof(RootRegistrationAttribute).FullName,
				Syntax.IsClassNode,
				ExtractRegistration
			);
				
		context.RegisterSourceOutput(incrementProvider, GenerateSources);
	}

	private RootRegistrationIncrement? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var namespaceName = symbol.ToNamespace();
		if (string.IsNullOrEmpty(namespaceName))
		{
			return null;
		}

		var className = symbol.Name;
		if (string.IsNullOrEmpty(className))
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<RootRegistrationAttribute>();
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 1)
		{
			return null;
		}

		if (constructorArguments[0].Values.IsEmpty)
		{
			return null;
		}

		var registrations = constructorArguments[0].Values
			.Select(typedConstant => typedConstant.Value)
			.OfType<INamedTypeSymbol>()
			.Select(symbol => Sources.GetCombinedRegistrationClassName(symbol.ContainingAssembly.Name))
			.ToImmutableArray();

		return new RootRegistrationIncrement(namespaceName, className, registrations);
	}

	private void GenerateSources(SourceProductionContext context, RootRegistrationIncrement? increment)
	{
		if (increment is null)
		{
			return;
		}

		var sourceBuilder = new RootRegistrationSourceBuilder(increment);
		var source = sourceBuilder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);
		
		context.AddSource(Sources.GeneratedNameHint(increment.ClassName), sourceText);
	}
}