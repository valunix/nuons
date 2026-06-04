using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Nuons.CodeInjection.Analyzers;

/// <summary>
/// NUCI007: reports nested classes annotated with <c>[InjectConstructor]</c>. Nested classes are not
/// supported by the generator and are skipped, so no constructor is produced for them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NestedClassAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUCI007";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Nested class with [InjectConstructor] is not supported",
		messageFormat: "Class '{0}' is marked with [InjectConstructor] but is nested; nested classes are not supported and no constructor will be generated",
		category: CodeInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

	/// <inheritdoc />
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

		if (symbol.ContainingType is null)
		{
			return;
		}

		var hasInjectConstructorAttribute = symbol.GetAttributes()
			.Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, analyzerContext.InjectConstructorAttributes));
		if (!hasInjectConstructorAttribute)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), symbol.Name);
		syntaxContext.ReportDiagnostic(diagnostic);
	}
}
