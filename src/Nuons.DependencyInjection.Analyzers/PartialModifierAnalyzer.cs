using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class PartialModifierAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NU001";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Class with [Target] must be partial",
		messageFormat: "Class '{0}' is marked as service registration but is not partial",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var nuonAnalyzerContext = new NuonAnalyzerContext(startContext.Compilation);
			startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeClass(syntaxContext, nuonAnalyzerContext), SyntaxKind.ClassDeclaration);
		});
	}

	private static void AnalyzeClass(SyntaxNodeAnalysisContext context, NuonAnalyzerContext nuonAnalyzerContext)
	{
		if (context.Node is not ClassDeclarationSyntax classDeclaration)
		{
			return;
		}

		if (context.ContainingSymbol is not { } symbol)
		{
			return;
		}

		var hasRegistrationAttribute = symbol.GetAttributes()
			.Any(attribute => nuonAnalyzerContext.ServiceAttributes.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));
		if (!hasRegistrationAttribute)
		{
			return;
		}

		if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		context.ReportDiagnostic(diagnostic);
	}
}
