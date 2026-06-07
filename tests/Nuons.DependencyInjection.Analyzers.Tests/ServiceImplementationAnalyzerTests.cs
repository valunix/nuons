using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Analyzers.Tests;

public class ServiceImplementationAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task GenericAttribute_ImplementedInterface_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IFoo;

[Singleton<IFoo>]
internal partial class TestService : IFoo;
";

		await fixture.VerifyAnalyzerAsync<ServiceImplementationAnalyzer>(testCode);
	}

	[Fact]
	public async Task GenericAttribute_UnrelatedInterface_ReportsError()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IFoo;
internal interface IBar;

[Singleton<IBar>]
internal partial class [|TestService|] : IFoo;
";

		await fixture.VerifyAnalyzerAsync<ServiceImplementationAnalyzer>(testCode);
	}

	[Fact]
	public async Task GenericAttribute_InheritedBaseClass_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal abstract class BaseService;

[Singleton<BaseService>]
internal partial class TestService : BaseService;
";

		await fixture.VerifyAnalyzerAsync<ServiceImplementationAnalyzer>(testCode);
	}

	[Fact]
	public async Task GenericAttribute_IndirectInterface_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IBase;
internal interface ITarget : IBase;

[Singleton<IBase>]
internal partial class TestService : ITarget;
";

		await fixture.VerifyAnalyzerAsync<ServiceImplementationAnalyzer>(testCode);
	}

	[Fact]
	public async Task ParameterlessAttribute_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal interface IFoo;

[Singleton]
internal partial class TestService : IFoo;
";

		await fixture.VerifyAnalyzerAsync<ServiceImplementationAnalyzer>(testCode);
	}
}
