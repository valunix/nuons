using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Analyzers;

/// <summary>
/// NUCI006: reports <c>[Injected]</c> or <c>[InjectedOptions]</c> fields that are not declared <c>readonly</c>.
/// Injected fields are assigned once in the generated constructor and should not be reassigned afterwards.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReadonlyInjectedFieldAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUCI006";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Injected field should be readonly",
		messageFormat: "Field '{0}' is marked as injected but is not readonly; injected fields are assigned only in the generated constructor",
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
			startContext.RegisterSymbolAction(symbolContext => AnalyzeField(symbolContext, nuonAnalyzerContext), SymbolKind.Field);
		});
	}

	private static void AnalyzeField(SymbolAnalysisContext symbolContext, CodeInjectionAnalyzerContext analyzerContext)
	{
		if (symbolContext.Symbol is not IFieldSymbol field)
		{
			return;
		}

		if (field.IsReadOnly || field.IsConst)
		{
			return;
		}

		if (!field.HasAnyAttribute(analyzerContext.InjectedAttributes))
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, field.Locations[0], field.Name);
		symbolContext.ReportDiagnostic(diagnostic);
	}
}
