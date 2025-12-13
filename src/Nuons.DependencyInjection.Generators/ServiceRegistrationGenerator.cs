using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;
using Nuons.DependencyInjection.Abstractions;

namespace Nuons.DependencyInjection.Generators;

// TODO improve to load attributes from compilation, filter target symbols and then combine and extract
[Generator]
internal class ServiceRegistrationGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((compilation, _) => compilation.AssemblyName);

		var transientProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(TransientAttribute).FullName,
			Syntax.IsClassNode,
			static (context, token) => ExtractRegistration(context, Lifetime.Transient, token)
		)
			.Collect();

		var genericTransientProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(TransientAttribute).FullName + Syntax.SingleGenericTypeSuffix,
			Syntax.IsClassNode,
			static (context, token) => ExtractGenericRegistration<TransientAttribute>(context, Lifetime.Transient, token)
		)
			.Collect();

		var scopedProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(ScopedAttribute).FullName,
			Syntax.IsClassNode,
			static (context, token) => ExtractRegistration(context, Lifetime.Scoped, token)
		)
			.Collect();

		var genericScopedProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(ScopedAttribute).FullName + Syntax.SingleGenericTypeSuffix,
			Syntax.IsClassNode,
			static (context, token) => ExtractGenericRegistration<ScopedAttribute>(context, Lifetime.Scoped, token)
		)
			.Collect();

		var singletonProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(SingletonAttribute).FullName,
			Syntax.IsClassNode,
			static (context, token) => ExtractRegistration(context, Lifetime.Singleton, token)
		)
			.Collect();

		var genericSingletonProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(SingletonAttribute).FullName + Syntax.SingleGenericTypeSuffix,
			Syntax.IsClassNode,
			static (context, token) => ExtractGenericRegistration<SingletonAttribute>(context, Lifetime.Singleton, token)
		)
			.Collect();

		var allRegistrations = transientProvider
			.Combine(genericTransientProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right))
			.Combine(scopedProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right))
			.Combine(genericScopedProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right))
			.Combine(singletonProvider)
			.Select(static (pair, _) => pair.Left.AddRange(pair.Right))
			.Combine(genericSingletonProvider)
			.SelectMany(static (pair, _) => pair.Left.AddRange(pair.Right))
			.WhereNotNull()
			.Collect();

		var combinedProvider = assemblyNameProvider.Combine(allRegistrations)
			.Select((pair, _) => new ServiceRegistrationIncrement(pair.Left, pair.Right));

		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private static ServiceRegistration? ExtractRegistration(GeneratorAttributeSyntaxContext context, Lifetime lifetime, CancellationToken token)
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

	private static ServiceRegistration? ExtractGenericRegistration<TAttribute>(GeneratorAttributeSyntaxContext context, Lifetime lifetime, CancellationToken token)
		where TAttribute : Attribute
	{
		if (context.TargetSymbol is not INamedTypeSymbol symbol)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<TAttribute>();
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
