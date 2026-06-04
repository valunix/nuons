using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Nuons.CodeInjection.Analyzers;

namespace Nuons.CodeInjection.CodeFixes;

/// <summary>
/// Code fix for NUCI002: adds the <c>[InjectConstructor]</c> attribute to a class that already has <c>[Injected]</c> or <c>[InjectedOptions]</c> fields.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InjectConstructorAttributeCodeFix))]
[Shared]
public sealed class InjectConstructorAttributeCodeFix : CodeFixProvider
{
	private const string Title = "Add [InjectConstructor] attribute";
	private const string AttributeFullName = "global::Nuons.CodeInjection.Abstractions.InjectConstructor";

	/// <inheritdoc />
	public override ImmutableArray<string> FixableDiagnosticIds => [MissingServiceAnalyzer.DiagnosticId];

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
					createChangedDocument: cancellationToken => AddInjectConstructorAttributeAsync(context.Document, classDeclaration, cancellationToken),
					equivalenceKey: nameof(InjectConstructorAttributeCodeFix)),
				diagnostic);
		}
	}

	private static async Task<Document> AddInjectConstructorAttributeAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
	{
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (root is null)
		{
			return document;
		}

		var firstToken = classDeclaration.GetFirstToken();

		var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(AttributeFullName))
			.WithAdditionalAnnotations(Simplifier.Annotation);
		var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
			.WithLeadingTrivia(firstToken.LeadingTrivia);

		var updated = classDeclaration
			.WithAttributeLists(classDeclaration.AttributeLists.Insert(0, attributeList));

		return document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, updated));
	}
}
