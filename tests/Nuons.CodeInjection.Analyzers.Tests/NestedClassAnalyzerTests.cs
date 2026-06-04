using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class NestedClassAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task NestedClassWithInjectConstructor_ReportsWarning()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public partial class Outer
{
	[InjectConstructor]
	public partial class [|Inner|];
}
";

		await fixture.VerifyAnalyzerAsync<NestedClassAnalyzer>(testCode);
	}

	[Fact]
	public async Task TopLevelClassWithInjectConstructor_NoWarning()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass;
";

		await fixture.VerifyAnalyzerAsync<NestedClassAnalyzer>(testCode);
	}

	[Fact]
	public async Task NestedClassWithoutInjectConstructor_NoWarning()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public partial class Outer
{
	public partial class Inner;
}
";

		await fixture.VerifyAnalyzerAsync<NestedClassAnalyzer>(testCode);
	}
}
