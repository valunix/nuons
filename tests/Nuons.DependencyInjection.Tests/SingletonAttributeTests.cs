using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class SingletonAttributeTests
{
	[Fact]
	public void SingletonAttribute_HasCorrectAttributeUsage()
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
	public void SingletonAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new SingletonAttribute(typeof(ITestService));

		// Assert
		attribute.ShouldNotBeNull();
	}
}
