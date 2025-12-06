using System.Reflection;

namespace Nuons.CodeInjection.Abstractions.Tests;

public class InjectConstructorAttributeTests
{
	[Fact]
	public void InjectConstructorAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(InjectConstructorAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void InjectConstructorAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new InjectConstructorAttribute();

		// Assert
		attribute.ShouldNotBeNull();
	}
}
