using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class MissingServiceAnalyzer : DiagnosticAnalyzer
{
	public const string DiagnosticId = "NU002";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Class with fields marked as [Injected] should be marked as service",
		messageFormat: "Class '{0}' has fields marked as Injected, it should be marked as service!",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Warning,
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

		if (context.ContainingSymbol is not INamedTypeSymbol symbol)
		{
			return;
		}

		var hasRegistrationAttribute = symbol.GetAttributes()
			.Any(attribute => nuonAnalyzerContext.ServiceAttributes.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));

		if (hasRegistrationAttribute)
		{
			return;
		}

		var hasInjectionAttribute = symbol.GetMembers()
			.Where(member => member.Kind == SymbolKind.Field)
			.SelectMany(member => member.GetAttributes())
			.Any(attribute => nuonAnalyzerContext.InjectedAttributes.Contains(attribute.AttributeClass, SymbolEqualityComparer.Default));

		if (!hasInjectionAttribute)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		context.ReportDiagnostic(diagnostic);
	}
}
