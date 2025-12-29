using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MultipleServiceAttributesAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NUDI001";

	private const int MaxServiceAttributes = 1;

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Multiple service registration attributes",
		messageFormat: "Class '{0}' has multiple service registration attributes: {1}",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "A class should be registered as service only once.");

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

		if (serviceAttributes.Count <= MaxServiceAttributes)
		{
			return;
		}

		var attributeNames = string.Join(", ", serviceAttributes.Select(attr => attr.AttributeClass!.Name));
		var diagnostic = Diagnostic.Create(
			Rule,
			classDeclaration.Identifier.GetLocation(),
			symbol.Name,
			attributeNames);

		syntaxContext.ReportDiagnostic(diagnostic);
	}
}
