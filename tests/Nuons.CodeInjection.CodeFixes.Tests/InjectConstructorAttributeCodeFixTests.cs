using Nuons.CodeInjection.Analyzers;
using Nuons.Core.Tests;

namespace Nuons.CodeInjection.CodeFixes.Tests;

public class InjectConstructorAttributeCodeFixTests(NuonCodeFixFixture fixture) : IClassFixture<NuonCodeFixFixture>
{
	[Fact]
	public async Task AddsInjectConstructorAttribute_WhenClassHasInjectedField()
	{
		const string source = @"
using Nuons.CodeInjection.Abstractions;

internal class [|TestClass|]
{
	[Injected]
	private readonly TestClass field;
}";

		const string fixedSource = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
internal class TestClass
{
	[Injected]
	private readonly TestClass field;
}";

		await fixture.VerifyCodeFixAsync<MissingServiceAnalyzer, InjectConstructorAttributeCodeFix>(source, fixedSource);
	}

	[Fact]
	public async Task AddsInjectConstructorAttribute_WhenClassHasInjectedOptionsField()
	{
		const string source = @"
using Nuons.CodeInjection.Abstractions;

internal class [|TestClass|]
{
	[InjectedOptions]
	private readonly TestClass field;
}";

		const string fixedSource = @"
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
internal class TestClass
{
	[InjectedOptions]
	private readonly TestClass field;
}";

		await fixture.VerifyCodeFixAsync<MissingServiceAnalyzer, InjectConstructorAttributeCodeFix>(source, fixedSource);
	}

	[Fact]
	public async Task AddsInjectConstructorAttribute_WhenClassAlreadyHasOtherAttribute()
	{
		const string source = @"
using System;
using Nuons.CodeInjection.Abstractions;

[Obsolete]
internal class [|TestClass|]
{
	[Injected]
	private readonly TestClass field;
}";

		const string fixedSource = @"
using System;
using Nuons.CodeInjection.Abstractions;

[InjectConstructor]
[Obsolete]
internal class TestClass
{
	[Injected]
	private readonly TestClass field;
}";

		await fixture.VerifyCodeFixAsync<MissingServiceAnalyzer, InjectConstructorAttributeCodeFix>(source, fixedSource);
	}
}
