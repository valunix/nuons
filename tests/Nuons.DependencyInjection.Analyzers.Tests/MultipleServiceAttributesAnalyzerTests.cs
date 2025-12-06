using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Analyzers.Tests;

public class MultipleServiceAttributesAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task ClassWithMultipleServiceAttributes_ReportsError()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface ITestService;

[Singleton]
[Scoped]
internal class [|TestService|] : ITestService;
";

		await fixture.VerifyAnalyzerAsync<MultipleServiceAttributesAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithSingleServiceAttribute_NoError()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface ITestService;

[Singleton]
internal class TestService : ITestService;
";

		await fixture.VerifyAnalyzerAsync<MultipleServiceAttributesAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithNoServiceAttributes_NoError()
	{
		const string testCode = @"
internal interface ITestService;
internal class TestService : ITestService;
";

		await fixture.VerifyAnalyzerAsync<MultipleServiceAttributesAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithServiceAttributeAndOtherAttributes_NoError()
	{
		const string testCode = @"
using System;
using Nuons.DependencyInjection.Abstractions;

internal interface ITestService;

[Singleton]
[Obsolete]
internal class TestService : ITestService;
";

		await fixture.VerifyAnalyzerAsync<MultipleServiceAttributesAnalyzer>(testCode);
	}
}
