using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

/// <summary>
/// NUDI002: warns when a parameterless lifetime attribute is applied to a class that directly implements more than one interface.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MultipleInterfacesAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUDI002";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Parameterless service attribute on class with multiple direct interfaces",
		messageFormat: "Class '{0}' implements multiple interfaces directlyl, use generic attribute to select target interface (e.g. [Singleton<IFoo>]).",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "When a class directly implements multiple interfaces, parameterless service registration attributes are ambiguous. Use the generic attribute to specify which interface to register.");

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

		var serviceAttributes = symbol.GetServiceAttributes(analyzerContext);

		// we only activate this analyzer when there is exactly one service registration attribute
		// if there are multiple we skip check as this is not valid and will trigger a different error diagnostic
		if (serviceAttributes.Count != 1)
		{
			return;
		}

		var serviceAttribute = serviceAttributes.First();
		if (serviceAttribute.AttributeClass!.IsGenericType)
		{
			return;
		}

		if (symbol.Interfaces.Length <= 1)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);
		symbolContext.ReportDiagnostic(diagnostic);
	}
}
