using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nuons.CodeInjection.Analyzers;

namespace Nuons.CodeInjection.CodeFixes;

/// <summary>
/// Code fix for NUCI006: adds the <c>readonly</c> modifier to an injected field.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReadonlyInjectedFieldCodeFix))]
[Shared]
public sealed class ReadonlyInjectedFieldCodeFix : CodeFixProvider
{
	private const string Title = "Add 'readonly' modifier";

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => [ReadonlyInjectedFieldAnalyzer.DiagnosticId];

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
			var fieldDeclaration = root
				.FindNode(diagnostic.Location.SourceSpan)
				.AncestorsAndSelf()
				.OfType<FieldDeclarationSyntax>()
				.FirstOrDefault();
			if (fieldDeclaration is null)
			{
				continue;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					title: Title,
					createChangedDocument: token => AddReadonlyModifierAsync(context.Document, fieldDeclaration, token),
					equivalenceKey: nameof(ReadonlyInjectedFieldCodeFix)),
				diagnostic);
		}
	}

	private static async Task<Document> AddReadonlyModifierAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
		{
			return document;
		}

		var readonlyToken = SyntaxFactory.Token(
			SyntaxFactory.TriviaList(),
			SyntaxKind.ReadOnlyKeyword,
			SyntaxFactory.TriviaList(SyntaxFactory.Space));
		var updated = fieldDeclaration.AddModifiers(readonlyToken);

		return document.WithSyntaxRoot(root.ReplaceNode(fieldDeclaration, updated));
	}
}
