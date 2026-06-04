using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Nuons.CodeInjection.Analyzers;

namespace Nuons.CodeInjection.CodeFixes;

/// <summary>
/// Code fix for NUCI001: adds the <c>partial</c> modifier to a class marked with <c>[InjectConstructor]</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InjectConstructorPartialModifierCodeFix))]
[Shared]
public sealed class InjectConstructorPartialModifierCodeFix : CodeFixProvider
{
	private const string Title = "Add 'partial' modifier";

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => [PartialModifierAnalyzer.DiagnosticId];

	/// <inheritdoc />
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
		{
			return;
		}

		foreach (var diagnostic in context.Diagnostics)
		{
			var classDeclaration = root
				.FindNode(diagnostic.Location.SourceSpan)
				.AncestorsAndSelf()
				.OfType<ClassDeclarationSyntax>()
				.FirstOrDefault();
			if (classDeclaration is null)
			{
				continue;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					title: Title,
					createChangedDocument: token => AddPartialModifierAsync(context.Document, classDeclaration, token),
					equivalenceKey: nameof(InjectConstructorPartialModifierCodeFix)),
				diagnostic);
		}
	}

	private static async Task<Document> AddPartialModifierAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
		{
			return document;
		}

		var partialToken = SyntaxFactory.Token(
			SyntaxFactory.TriviaList(),
			SyntaxKind.PartialKeyword,
			SyntaxFactory.TriviaList(SyntaxFactory.Space));
		var updated = classDeclaration
			.AddModifiers(partialToken)
			.WithAdditionalAnnotations(Formatter.Annotation);
		return document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, updated));
	}
}
