using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class PartialModifierAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task ClassWithServiceAttribute_MissingPartialModifier_ReportsError()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public class [|TestClass|];
";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithPartialModifier_NoError()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass;
";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithoutServiceAttribute_NoError()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public class TestClass;
";

		await fixture.VerifyAnalyzerAsync<PartialModifierAnalyzer>(testCode);
	}
}
