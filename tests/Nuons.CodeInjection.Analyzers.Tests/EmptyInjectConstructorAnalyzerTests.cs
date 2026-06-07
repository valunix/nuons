using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class EmptyInjectConstructorAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task InjectConstructor_WithoutInjectedFields_ReportsInfo()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class [|TestClass|]
{
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<EmptyInjectConstructorAnalyzer>(testCode);
	}

	[Fact]
	public async Task InjectConstructor_WithInjectedField_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass
{
	[Injected]
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<EmptyInjectConstructorAnalyzer>(testCode);
	}

	[Fact]
	public async Task InjectConstructor_WithInjectedOptionsField_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public class MyOptions { }

[InjectConstructor]
public partial class TestClass
{
	[InjectedOptions]
	private readonly MyOptions field;
}";

		await fixture.VerifyAnalyzerAsync<EmptyInjectConstructorAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithoutInjectConstructor_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public partial class TestClass
{
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<EmptyInjectConstructorAnalyzer>(testCode);
	}
}
