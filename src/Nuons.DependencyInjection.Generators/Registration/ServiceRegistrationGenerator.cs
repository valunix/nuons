using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Generators.Registration;

[Generator]
internal class ServiceRegistrationGenerator : IIncrementalGenerator
{

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var assemblyNameProvider = context.CompilationProvider
			.Select((c, _) => c.AssemblyName);

		var serviceRegistrationsProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
			typeof(ServiceAttribute).FullName,
			Syntax.IsClassNode,
			ExtractRegistration
		)
			.Where(r => r is not null)
			.Collect();

		var combinedProvider = assemblyNameProvider.Combine(serviceRegistrationsProvider)
			.Select((combined, _) => new ServiceRegistrationIncrement(combined.Left, combined.Right));
		context.RegisterSourceOutput(combinedProvider, GenerateSources);
	}

	private ServiceLifetimeRegistration? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken token)
	{
		var symbol = context.TargetSymbol as INamedTypeSymbol;
		if (symbol is null)
		{
			return null;
		}

		var attribute = symbol.FirstOrDefaultAttribute<ServiceAttribute>();
		if (attribute is null)
		{
			return null;
		}

		var constructorArguments = attribute.ConstructorArguments;
		if (constructorArguments.Length is not 2)
		{
			return null;
		}

		if (constructorArguments[0].Value is not int lifetimeArg || !lifetimeArg.IsLifetime())
		{
			return null;
		}
		var lifetime = (Lifetime)lifetimeArg;

		if (constructorArguments[1].Value is not INamedTypeSymbol typeArgument)
		{
			return null;
		}
		var serviceType = typeArgument.ToFullName();

		return new ServiceLifetimeRegistration(serviceType, symbol.ToFullName(), lifetime);
	}

	private void GenerateSources(SourceProductionContext context, ServiceRegistrationIncrement increment)
	{
		if (increment.AssemblyName is null || !increment.Registrations.Any())
		{
			return;
		}

		var className = Sources.GetServiceRegistrationClassName(increment.AssemblyName);
		var sourceBuilder = new RegistrationSourceBuilder(className);
		foreach (var registration in increment.Registrations)
		{
			if (registration is null)
			{
				continue;
			}

			sourceBuilder.WithRegistration(registration);
		}

		var source = sourceBuilder.Build();
		var sourceText = SourceText.From(source, Encoding.UTF8);

		context.AddSource(Sources.GeneratedNameHint(className), sourceText);
	}
}
