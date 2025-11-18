namespace Nuons.DependencyInjection.Analyzers.Tests;

public class MultipleInterfacesAnalyzerTests(NuonsAnalyzerFixture fixture) : IClassFixture<NuonsAnalyzerFixture>
{
	[Fact]
	public async Task ParameterlessAttribute_WithMultipleDirectInterfaces_ReportsWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IFirst;
internal interface ISecond;

[Singleton]
internal partial class [|TestService|] : IFirst, ISecond;";

		await fixture.VerifyAnalyzerAsync<MultipleInterfacesAnalyzer>(testCode);
	}

	[Fact]
	public async Task ParameterlessAttribute_WithSingleDirectInterface_NoWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface ITestService;

[Singleton]
internal partial class TestService : ITestService;";

		await fixture.VerifyAnalyzerAsync<MultipleInterfacesAnalyzer>(testCode);
	}

	[Fact]
	public async Task GenericAttribute_WithMultipleDirectInterfaces_NoWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IFirst;
internal interface ISecond;

[Singleton<IFirst>]
internal partial class TestService : IFirst, ISecond;";

		await fixture.VerifyAnalyzerAsync<MultipleInterfacesAnalyzer>(testCode);
	}

	[Fact]
	public async Task ParameterlessAttribute_WithIndirectInterface_NoWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IBase;
internal interface ITarget : IBase;

[Singleton]
internal partial class TestService : ITarget;";

		await fixture.VerifyAnalyzerAsync<MultipleInterfacesAnalyzer>(testCode);
	}
}
