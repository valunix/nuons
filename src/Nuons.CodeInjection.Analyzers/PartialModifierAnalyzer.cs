using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.CodeInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class PartialModifierAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NU001";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Class with [InjectConstructor] must be partial",
		messageFormat: "Class '{0}' is marked with [InjectConstructor] but is not partial",
		category: CodeInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var nuonAnalyzerContext = new CodeInjectionAnalyzerContext(startContext.Compilation);
			startContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeClass(syntaxContext, nuonAnalyzerContext), SyntaxKind.ClassDeclaration);
		});
	}

	private static void AnalyzeClass(SyntaxNodeAnalysisContext syntaxContext, CodeInjectionAnalyzerContext analyzerContext)
	{
		if (syntaxContext.Node is not ClassDeclarationSyntax classDeclaration)
		{
			return;
		}

		if (syntaxContext.ContainingSymbol is not { } symbol)
		{
			return;
		}

		var hasRegistrationAttribute = symbol.GetAttributes()
			.Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, analyzerContext.InjectConstructorAttributes));
		if (!hasRegistrationAttribute)
		{
			return;
		}

		if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		syntaxContext.ReportDiagnostic(diagnostic);
	}
}
