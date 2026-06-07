using Nuons.CodeInjection.Analyzers;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.CodeFixes.Tests;

public class ReadonlyInjectedFieldCodeFixTests(NuonCodeFixFixture fixture) : IClassFixture<NuonCodeFixFixture>
{
	[Fact]
	public async Task AddsReadonlyModifier_ToInjectedField()
	{
		const string source = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass
{
	[Injected]
	private string [|field|];
}";

		const string fixedSource = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass
{
	[Injected]
	private readonly string field;
}";

		await fixture.VerifyCodeFixAsync<ReadonlyInjectedFieldAnalyzer, ReadonlyInjectedFieldCodeFix>(source, fixedSource);
	}
}
