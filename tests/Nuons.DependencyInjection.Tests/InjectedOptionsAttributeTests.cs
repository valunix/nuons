using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class InjectedOptionsAttributeTests
{
	[Fact]
	public void InjectedOptionsAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(InjectedOptionsAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Field);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void InjectedOptionsAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new InjectedOptionsAttribute();

		// Assert
		attribute.ShouldNotBeNull();
	}
}
