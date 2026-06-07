using Nuons.Core.Tests;

namespace Nuons.DependencyInjection.Analyzers.Tests;

public class OptionsSectionKeyAnalyzerTests(NuonAnalyzerFixture fixture) : IClassFixture<NuonAnalyzerFixture>
{
	[Fact]
	public async Task EmptySectionKey_ReportsWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

[[|Options("""")|]]
internal partial class TestOptions;
";

		await fixture.VerifyAnalyzerAsync<OptionsSectionKeyAnalyzer>(testCode);
	}

	[Fact]
	public async Task WhitespaceSectionKey_ReportsWarning()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

[[|Options(""   "")|]]
internal partial class TestOptions;
";

		await fixture.VerifyAnalyzerAsync<OptionsSectionKeyAnalyzer>(testCode);
	}

	[Fact]
	public async Task NonEmptySectionKey_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

[Options(""MySection"")]
internal partial class TestOptions;
";

		await fixture.VerifyAnalyzerAsync<OptionsSectionKeyAnalyzer>(testCode);
	}

	[Fact]
	public async Task ClassWithoutOptionsAttribute_NoDiagnostic()
	{
		const string testCode = @"
using Nuons.DependencyInjection.Abstractions;

internal partial class TestOptions;
";

		await fixture.VerifyAnalyzerAsync<OptionsSectionKeyAnalyzer>(testCode);
	}
}
