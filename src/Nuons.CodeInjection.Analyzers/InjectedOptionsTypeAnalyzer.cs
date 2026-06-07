using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Analyzers;

/// <summary>
/// NUCI003: reports <c>[InjectedOptions]</c> fields already typed as <c>IOptions&lt;T&gt;</c>. The generator wraps the
/// field type in <c>IOptions&lt;&gt;</c>, so such a field would produce <c>IOptions&lt;IOptions&lt;T&gt;&gt;</c>; the field should be typed as <c>T</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InjectedOptionsTypeAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUCI003";

	// The generator emits a parameter of global::Microsoft.Extensions.Options.IOptions<field-type>; matching the
	// original definition by display string avoids requiring the options package to be referenced by the analyzer.
	internal const string OptionsInterfaceDisplay = "Microsoft.Extensions.Options.IOptions<T>";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "[InjectedOptions] field must not be typed as IOptions<T>",
		messageFormat: "Field '{0}' is marked with [InjectedOptions] but typed as IOptions<T>; use the options type directly as the generator wraps it in IOptions<>",
		category: CodeInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Error,
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

		if (!field.HasAttribute(analyzerContext.InjectedOptionsAttribute))
		{
			return;
		}

		if (field.Type is not INamedTypeSymbol { IsGenericType: true } namedType)
		{
			return;
		}

		if (namedType.OriginalDefinition.ToDisplayString() != OptionsInterfaceDisplay)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, field.Locations[0], field.Name);
		symbolContext.ReportDiagnostic(diagnostic);
	}
}
