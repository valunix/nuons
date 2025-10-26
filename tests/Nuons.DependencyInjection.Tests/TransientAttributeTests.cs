using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class TransientAttributeTests
{
	[Fact]
	public void TransientAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(TransientAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void TransientAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new TransientAttribute(typeof(ITestService));

		// Assert
		attribute.ShouldNotBeNull();
	}
}
