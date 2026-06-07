using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Analyzers.Tests;

public class InjectedOptionsTypeAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	// Stubs the options interface so the analyzer can match it by name without referencing the package.
	private const string OptionsStub = @"
namespace Microsoft.Extensions.Options
{
	public interface IOptions<out T> where T : class
	{
		T Value { get; }
	}
}
";

	[Fact]
	public async Task InjectedOptions_TypedAsIOptions_ReportsError()
	{
		string testCode = $@"
using Nuons.CodeInjection.Abstractions;
using Microsoft.Extensions.Options;
{OptionsStub}
public class MyOptions {{ }}

[InjectConstructor]
public partial class TestClass
{{
	[InjectedOptions]
	private readonly IOptions<MyOptions> [|field|];
}}";

		await fixture.VerifyAnalyzerAsync<InjectedOptionsTypeAnalyzer>(testCode);
	}

	[Fact]
	public async Task InjectedOptions_TypedAsOptionType_NoDiagnostic()
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

		await fixture.VerifyAnalyzerAsync<InjectedOptionsTypeAnalyzer>(testCode);
	}

	[Fact]
	public async Task PlainInjected_TypedAsIOptions_NoDiagnostic()
	{
		string testCode = $@"
using Nuons.CodeInjection.Abstractions;
using Microsoft.Extensions.Options;
{OptionsStub}
public class MyOptions {{ }}

[InjectConstructor]
public partial class TestClass
{{
	[Injected]
	private readonly IOptions<MyOptions> field;
}}";

		await fixture.VerifyAnalyzerAsync<InjectedOptionsTypeAnalyzer>(testCode);
	}
}
