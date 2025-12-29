using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MultipleInterfacesAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NUDI002";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Parameterless service attribute on class with multiple direct interfaces",
		messageFormat: "Class '{0}' implements multiple interfaces directlyl, use generic attribute to select target interface (e.g. [Singleton<IFoo>]).",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "When a class directly implements multiple interfaces, parameterless service registration attributes are ambiguous. Use the generic attribute to specify which interface to register.");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var nuonAnalyzerContext = new DependencyInjectionAnalyzerContext(startContext.Compilation);
			startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeClass(syntaxContext, nuonAnalyzerContext), SyntaxKind.ClassDeclaration);
		});
	}

	private static void AnalyzeClass(SyntaxNodeAnalysisContext syntaxContext, DependencyInjectionAnalyzerContext analyzerContext)
	{
		if (syntaxContext.Node is not ClassDeclarationSyntax classDeclaration)
		{
			return;
		}

		if (syntaxContext.ContainingSymbol is not INamedTypeSymbol symbol)
		{
			return;
		}

		var serviceAttributes = symbol.GetAttributes()
			.Where(attribute => attribute.AttributeClass is not null
				&& analyzerContext.ServiceAttributes.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default))
			.ToList();

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

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		syntaxContext.ReportDiagnostic(diagnostic);
	}
}
