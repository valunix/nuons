using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class RootRegistrationAttributeTests
{
	[Fact]
	public void RootRegistrationAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(SingletonAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void RootRegistrationAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new RootRegistrationAttribute(typeof(RootRegistrationAttributeTests));

		// Assert
		attribute.ShouldNotBeNull();
	}
}
