using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class InjectedConstructorAttributeTests
{
	[Fact]
	public void InjectedConstructorAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(InjectedConstructorAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void InjectedConstructorAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new InjectedConstructorAttribute();

		// Assert
		attribute.ShouldNotBeNull();
	}
}
