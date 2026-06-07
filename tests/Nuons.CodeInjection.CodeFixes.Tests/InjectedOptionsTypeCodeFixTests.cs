using Nuons.CodeInjection.Analyzers;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.CodeFixes.Tests;

public class InjectedOptionsTypeCodeFixTests(NuonCodeFixFixture fixture) : IClassFixture<NuonCodeFixFixture>
{
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
	public async Task ReplacesIOptionsWithInnerType()
	{
		string source = $@"
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

		string fixedSource = $@"
using Nuons.CodeInjection.Abstractions;
using Microsoft.Extensions.Options;
{OptionsStub}
public class MyOptions {{ }}

[InjectConstructor]
public partial class TestClass
{{
	[InjectedOptions]
	private readonly MyOptions field;
}}";

		await fixture.VerifyCodeFixAsync<InjectedOptionsTypeAnalyzer, InjectedOptionsTypeCodeFix>(source, fixedSource);
	}
}
