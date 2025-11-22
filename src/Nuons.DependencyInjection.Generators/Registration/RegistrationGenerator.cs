using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators.Registration;

internal abstract class RegistrationGenerator<T>(Lifetime lifetime) : IIncrementalGenerator
	where T : Attribute
{
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

		var genericServiceRegistrationsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(T).FullName + Syntax.SingleGenericTypeSuffix,
			Syntax.IsClassNode,
			ExtractGenericRegistration
		)
			.Where(registration => registration is not null)
			.Collect();

		var allRegistrations = serviceRegistrationsProvider
			.Combine(genericServiceRegistrationsProvider)
			.SelectMany((pair, _) => pair.Left.AddRange(pair.Right))
			.Collect();

		var combinedProvider = assemblyNameProvider.Combine(allRegistrations)
			.Select((pair, _) => new RegistrationIncrement(pair.Left, pair.Right));

		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private ServiceRegistration? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<T>();
		if (attribute is null)
		{
			return null;
		}

		INamedTypeSymbol serviceTypeSymbol;
		if (symbol.Interfaces.Length == 1)
		{
			serviceTypeSymbol = symbol.Interfaces[0];
		}
		else
		{
			serviceTypeSymbol = symbol;
		}

		var serviceType = serviceTypeSymbol.ToFullTypeName();
		var implementationType = symbol.ToFullTypeName();

		return new ServiceRegistration(serviceType, implementationType);
	}

	private ServiceRegistration? ExtractGenericRegistration(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<T>();
		if (attribute is null)
		{
			return null;
		}

		var attributeClass = attribute.AttributeClass;
		if (attributeClass is null || attributeClass.TypeArguments.Length != 1)
		{
			return null;
		}

		var typeArgument = attributeClass.TypeArguments[0];
		if (typeArgument is not INamedTypeSymbol namedType)
		{
			return null;
		}

		var serviceType = namedType.ToFullTypeName();
		var implementationType = symbol.ToFullTypeName();

		return new ServiceRegistration(serviceType, implementationType);
	}

	private void GenerateSources(SourceProductionContext context, RegistrationIncrement increment)
	{
		if (increment.AssemblyName is null || !increment.Registrations.Any())
		{
			return;
		}

		var className = DependancyInjectionSources.GetLifetimeRegistrationClassName(increment.AssemblyName, lifetime);
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
