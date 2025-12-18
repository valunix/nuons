using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators;

[Generator]
internal class ServiceRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((compilation, _) => compilation.AssemblyName);

		var transientProvider = GetProviderFor<TransientAttribute>(context, Lifetime.Transient);
		var scopedProvider = GetProviderFor<ScopedAttribute>(context, Lifetime.Scoped);
		var singletonProvider = GetProviderFor<SingletonAttribute>(context, Lifetime.Singleton);

		var allRegistrations = transientProvider
			.Combine(scopedProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right))
			.Combine(singletonProvider)
			.SelectMany(static (pair, _) => pair.Left.AddRange(pair.Right))
			.WhereNotNull()
			.Collect();

		var combinedProvider = assemblyNameProvider.Combine(allRegistrations)
			.Select((pair, _) => new ServiceRegistrationIncrement(pair.Left, pair.Right));

		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private IncrementalValueProvider<ImmutableArray<ServiceRegistration?>> GetProviderFor<TAttribute>(IncrementalGeneratorInitializationContext context, Lifetime lifetime)
	{
		var attributeFullName = typeof(TAttribute).FullName;
		var provider = context.SyntaxProvider.ForAttributeWithMetadataName
		(
			attributeFullName,
			Syntax.IsClassNode,
			(context, _) => ExtractRegistration(context, lifetime)
		)
			.Collect();

		var genericProvider = context.SyntaxProvider.ForAttributeWithMetadataName
		(
			attributeFullName + Syntax.SingleGenericTypeSuffix,
			Syntax.IsClassNode,
			(context, _) => ExtractGenericRegistration(context, attributeFullName, lifetime)
		)
			.Collect();

		return provider
			.Combine(genericProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right));
	}

	private static ServiceRegistration? ExtractRegistration(GeneratorAttributeSyntaxContext context, Lifetime lifetime)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
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

		return new ServiceRegistration(serviceType, implementationType, lifetime);
	}

	private static ServiceRegistration? ExtractGenericRegistration(GeneratorAttributeSyntaxContext context, string attributeFullName, Lifetime lifetime)
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute(attributeFullName);
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

		return new ServiceRegistration(serviceType, implementationType, lifetime);
	}

	private void GenerateSources(SourceProductionContext context, ServiceRegistrationIncrement increment)
	{
		if (increment.AssemblyName is null)
		{
			return;
		}

		var className = DependancyInjectionSources.GetServiceRegistrationClassName(increment.AssemblyName);
		var sourceBuilder = new ServiceRegistrationSourceBuilder(className);
		foreach (var registration in increment.Registrations)
		{
			sourceBuilder.WithRegistration(registration);
		}

		var source = sourceBuilder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}
