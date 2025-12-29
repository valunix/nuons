using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.CodeInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MissingServiceAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NUCI002";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Class with fields marked as [Injected] should be marked with [InjectConstructor]",
		messageFormat: "Class '{0}' has fields marked as Injected, it should be marked with [InjectConstructor]!",
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

		if (syntaxContext.ContainingSymbol is not INamedTypeSymbol symbol)
		{
			return;
		}

		var hasRegistrationAttribute = symbol.GetAttributes()
			.Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, analyzerContext.InjectConstructorAttributes));

		if (hasRegistrationAttribute)
		{
			return;
		}

		var hasInjectionAttribute = symbol.GetMembers()
			.Where(member => member.Kind == SymbolKind.Field)
			.SelectMany(member => member.GetAttributes())
			.Any(attribute => analyzerContext.InjectedAttributes.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));

		if (!hasInjectionAttribute)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		syntaxContext.ReportDiagnostic(diagnostic);
	}
}
