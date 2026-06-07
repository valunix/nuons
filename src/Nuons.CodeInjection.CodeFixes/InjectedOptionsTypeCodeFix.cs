using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nuons.CodeInjection.Analyzers;

namespace Nuons.CodeInjection.CodeFixes;

/// <summary>
/// Code fix for NUCI003: replaces an <c>IOptions&lt;T&gt;</c> field type with the inner <c>T</c> so the generator wraps it correctly.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InjectedOptionsTypeCodeFix))]
[Shared]
public sealed class InjectedOptionsTypeCodeFix : CodeFixProvider
{
	private const string Title = "Replace IOptions<T> with T";

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => [InjectedOptionsTypeAnalyzer.DiagnosticId];

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

			// The field type may be a bare GenericName (IOptions<T>) or a qualified one (Microsoft.Extensions.Options.IOptions<T>).
			var optionsType = fieldDeclaration?.Declaration.Type
				.DescendantNodesAndSelf()
				.OfType<GenericNameSyntax>()
				.FirstOrDefault(generic => generic.TypeArgumentList.Arguments.Count == 1);
			if (optionsType is null)
			{
				continue;
			}

			var fieldType = fieldDeclaration!.Declaration.Type;
			context.RegisterCodeFix(
				CodeAction.Create(
					title: Title,
					createChangedDocument: token => ReplaceWithInnerTypeAsync(context.Document, fieldType, optionsType, token),
					equivalenceKey: nameof(InjectedOptionsTypeCodeFix)),
				diagnostic);
		}
	}

	private static async Task<Document> ReplaceWithInnerTypeAsync(Document document, TypeSyntax fieldType, GenericNameSyntax optionsType, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
		{
			return document;
		}

		var innerType = optionsType.TypeArgumentList.Arguments[0]
			.WithTriviaFrom(fieldType);

		return document.WithSyntaxRoot(root.ReplaceNode(fieldType, innerType));
	}
}
