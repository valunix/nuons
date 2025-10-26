using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class OptionsAttributeTests
{
	[Fact]
	public void OptionsAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(OptionsAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void OptionsAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new OptionsAttribute("TestSection");

		// Assert
		attribute.ShouldNotBeNull();
	}
}
