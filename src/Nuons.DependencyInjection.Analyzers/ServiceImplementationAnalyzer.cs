using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

/// <summary>
/// NUDI003: reports a generic lifetime attribute (e.g. <c>[Singleton&lt;TService&gt;]</c>) whose class does not implement or inherit the requested service type.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ServiceImplementationAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUDI003";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Service type is not implemented by the registered class",
		messageFormat: "Class '{0}' is registered as '{1}' but does not implement or inherit it",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "A generic lifetime attribute must target a type that the annotated class implements or inherits.");

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var nuonAnalyzerContext = new DependencyInjectionAnalyzerContext(startContext.Compilation);
			startContext.RegisterSymbolAction(symbolContext => AnalyzeClass(symbolContext, nuonAnalyzerContext), SymbolKind.NamedType);
		});
	}

	private static void AnalyzeClass(SymbolAnalysisContext symbolContext, DependencyInjectionAnalyzerContext analyzerContext)
	{
		if (symbolContext.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } symbol)
		{
			return;
		}

		foreach (var attribute in symbol.GetServiceAttributes(analyzerContext))
		{
			if (attribute.AttributeClass is not { IsGenericType: true } attributeClass)
			{
				continue;
			}

			var serviceType = attributeClass.TypeArguments[0];

			// Skip unresolved type arguments; another diagnostic already covers those.
			if (serviceType.TypeKind == TypeKind.Error)
			{
				continue;
			}

			if (IsAssignableTo(symbol, serviceType))
			{
				continue;
			}

			var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, serviceType.ToDisplayString());
			symbolContext.ReportDiagnostic(diagnostic);
		}
	}

	private static bool IsAssignableTo(INamedTypeSymbol implementation, ITypeSymbol serviceType)
	{
		if (SymbolEqualityComparer.Default.Equals(implementation, serviceType))
		{
			return true;
		}

		if (serviceType.TypeKind == TypeKind.Interface)
		{
			return implementation.AllInterfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, serviceType));
		}

		for (var baseType = implementation.BaseType; baseType is not null; baseType = baseType.BaseType)
		{
			if (SymbolEqualityComparer.Default.Equals(baseType, serviceType))
			{
				return true;
			}
		}

		return false;
	}
}
