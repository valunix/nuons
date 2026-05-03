using Nuons.CodeInjection.Analyzers;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.CodeFixes.Tests;

public class InjectConstructorPartialModifierCodeFixTests(NuonCodeFixFixture fixture) : IClassFixture<NuonCodeFixFixture>
{
	[Fact]
	public async Task AddsPartialModifier_WhenClassIsMissingIt()
	{
		const string source = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public class [|TestClass|];
";

		const string fixedSource = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass;
";

		await fixture.VerifyCodeFixAsync<PartialModifierAnalyzer, InjectConstructorPartialModifierCodeFix>(source, fixedSource);
	}

	[Fact]
	public async Task AddsPartialModifier_AfterExistingSealedModifier()
	{
		const string source = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
internal sealed class [|TestClass|];
";

		const string fixedSource = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
internal sealed partial class TestClass;
";

		await fixture.VerifyCodeFixAsync<PartialModifierAnalyzer, InjectConstructorPartialModifierCodeFix>(source, fixedSource);
	}
}
