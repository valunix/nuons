using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class ReadonlyInjectedFieldAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task InjectedField_NotReadonly_ReportsWarning()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass
{
	[Injected]
	private string [|field|];
}";

		await fixture.VerifyAnalyzerAsync<ReadonlyInjectedFieldAnalyzer>(testCode);
	}

	[Fact]
	public async Task InjectedOptionsField_NotReadonly_ReportsWarning()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public class MyOptions { }

[InjectConstructor]
public partial class TestClass
{
	[InjectedOptions]
	private MyOptions [|field|];
}";

		await fixture.VerifyAnalyzerAsync<ReadonlyInjectedFieldAnalyzer>(testCode);
	}

	[Fact]
	public async Task InjectedField_Readonly_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
public partial class TestClass
{
	[Injected]
	private readonly string field;
}";

		await fixture.VerifyAnalyzerAsync<ReadonlyInjectedFieldAnalyzer>(testCode);
	}

	[Fact]
	public async Task PlainField_NotReadonly_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.CodeInjection.Abstractions;

public partial class TestClass
{
	private string field;
}";

		await fixture.VerifyAnalyzerAsync<ReadonlyInjectedFieldAnalyzer>(testCode);
	}
}
