using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Generators;

namespace Nuons.DependencyInjection.Analyzers;

/// <summary>
/// NUDI004: warns when the configuration section key passed to <c>[Options(...)]</c> is empty or whitespace,
/// which binds the options to the configuration root rather than a named section.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OptionsSectionKeyAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUDI004";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "Options section key is empty",
		messageFormat: "Class '{0}' is marked with [Options] but the section key is empty or whitespace",
		category: DependencyInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		description: "An empty section key binds the options to the configuration root; provide a section key.");

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

		var optionsAttribute = symbol.FirstOrDefaultAttribute(analyzerContext.OptionsAttribute);
		if (optionsAttribute is null)
		{
			return;
		}

		if (optionsAttribute.ConstructorArguments.Length != 1)
		{
			return;
		}

		var sectionKey = optionsAttribute.ConstructorArguments[0].Value as string;
		if (!string.IsNullOrWhiteSpace(sectionKey))
		{
			return;
		}

		var location = optionsAttribute.ApplicationSyntaxReference?.GetSyntax(symbolContext.CancellationToken).GetLocation()
			?? symbol.Locations[0];
		var diagnostic = Diagnostic.Create(Rule, location, symbol.Name);
		symbolContext.ReportDiagnostic(diagnostic);
	}
}
