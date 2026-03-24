using Nuons.Core.Tests;

namespace Nuons.CodeInjection.Generators.Tests;

public class DtoMappingGeneratorTests(ITestOutputHelper output, NuonGeneratorFixture fixture)
	: IClassFixture<NuonGeneratorFixture>
{
	[Fact]
	public Task AllPublicPropertiesAreMapped()
	{
		var sources = fixture.GenerateMappingSources("AllPublicPropertiesAreMapped.cs");
		return Verify(sources);
	}

	[Fact]
	public Task MapIgnoreExcludesProperties()
	{
		var sources = fixture.GenerateMappingSources("MapIgnoreExcludesProperties.cs");
		return Verify(sources);
	}

	[Fact]
	public Task NullablePropertiesArePreserved()
	{
		var sources = fixture.GenerateMappingSources("NullablePropertiesArePreserved.cs");
		return Verify(sources);
	}

	[Fact]
	public Task EnumPropertiesAreMapped()
	{
		var sources = fixture.GenerateMappingSources("EnumPropertiesAreMapped.cs");
		return Verify(sources);
	}

	[Fact]
	public Task UnsupportedTypesAreSkipped()
	{
		var sources = fixture.GenerateMappingSources("UnsupportedTypesAreSkipped.cs");
		return Verify(sources);
	}

	[Fact]
	public Task ReadOnlySourcePropertiesAreMapped()
	{
		var sources = fixture.GenerateMappingSources("ReadOnlySourcePropertiesAreMapped.cs");
		return Verify(sources);
	}

	[Fact]
	public Task ExtensionMethodIsGeneratedCorrectly()
	{
		var sources = fixture.GenerateMappingSources("ExtensionMethodIsGeneratedCorrectly.cs");
		return Verify(sources);
	}

	[Fact]
	public void NonPartialRecordIsSkipped()
	{
		var sources = fixture.GenerateMappingSources("NonPartialRecordIsSkipped.cs");
		Assert.Equal(string.Empty, sources);
	}
}
