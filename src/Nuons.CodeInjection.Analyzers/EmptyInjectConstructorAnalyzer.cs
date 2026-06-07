using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Analyzers;

/// <summary>
/// NUCI004: reports classes annotated with <c>[InjectConstructor]</c> that declare no <c>[Injected]</c> or <c>[InjectedOptions]</c> fields.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyInjectConstructorAnalyzer : DiagnosticAnalyzer
{
	/// <summary>The diagnostic identifier reported by this analyzer.</summary>
	public const string DiagnosticId = "NUCI004";

	private static readonly DiagnosticDescriptor Rule = new(
		id: DiagnosticId,
		title: "[InjectConstructor] class has no injected fields",
		messageFormat: "Class '{0}' is marked with [InjectConstructor] but has no [Injected] or [InjectedOptions] fields; the generated constructor will be empty",
		category: CodeInjectionAnalyzers.Category,
		defaultSeverity: DiagnosticSeverity.Info,
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
			startContext.RegisterSymbolAction(symbolContext => AnalyzeClass(symbolContext, nuonAnalyzerContext), SymbolKind.NamedType);
		});
	}

	private static void AnalyzeClass(SymbolAnalysisContext symbolContext, CodeInjectionAnalyzerContext analyzerContext)
	{
		if (symbolContext.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } symbol)
		{
			return;
		}

		if (!symbol.HasAttribute(analyzerContext.InjectConstructorAttributes))
		{
			return;
		}

		var hasInjectedField = symbol.GetMembers()
			.OfType<IFieldSymbol>()
			.Any(field => field.HasAnyAttribute(analyzerContext.InjectedAttributes));
		if (hasInjectedField)
		{
			return;
		}

		var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);
		symbolContext.ReportDiagnostic(diagnostic);
	}
}
