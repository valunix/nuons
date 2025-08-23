using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Nuons.DependencyInjection.Generators.Registration;

internal abstract class RegistrationGenerator<T> : IIncrementalGenerator
	where T : Attribute
{
	private readonly Lifetime lifetime;

	protected RegistrationGenerator(Lifetime lifetime)
	{
		this.lifetime = lifetime;
	}

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((compilation, _) => compilation.AssemblyName);

		var serviceRegistrationsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(T).FullName,
			Syntax.IsClassNode,
			ExtractRegistration
		)
			.Where(registration => registration is not null)
			.Collect();

		var combinedProvider = assemblyNameProvider.Combine(serviceRegistrationsProvider)
			.Select((pair, _) => new RegistrationIncrement(pair.Left, pair.Right));

		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private ServiceRegistration? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol) return null;

		var attribute = symbol.FirstOrDefaultAttribute<T>();
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if(constructorArguments.Length is not 1)
		{
			return null;
		}

		if (constructorArguments[0].Value is not INamedTypeSymbol typeArgument)
		{
			return null;
		}

		var serviceType = typeArgument.ToFullName();
		return new ServiceRegistration(serviceType, symbol.ToFullName());
	}

	private void GenerateSources(SourceProductionContext context, RegistrationIncrement increment)
	{
		if (increment.AssemblyName is null || !increment.Registrations.Any())
		{
			return;
		}

		var className = Sources.GetLifetimeRegistrationClassName(increment.AssemblyName, lifetime);
		var sourceBuilder = new RegistrationSourceBuilder(className);
		foreach (var registration in increment.Registrations)
		{
			if (registration is null)
			{
				continue;
			}

			sourceBuilder.WithRegistration(registration, lifetime);
		}

		var source = sourceBuilder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}