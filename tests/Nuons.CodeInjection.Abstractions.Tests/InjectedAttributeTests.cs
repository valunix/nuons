using System.Reflection;

namespace Nuons.CodeInjection.Abstractions.Tests;

public class InjectedAttributeTests
{
	[Fact]
	public void InjectedAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(InjectedAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Field);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void InjectedAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new InjectedAttribute();

		// Assert
		attribute.ShouldNotBeNull();
	}
}
