using System.Reflection;

namespace Nuons.DependencyInjection.Abstractions.Tests;

public class ServiceAttributeTests
{
	[Fact]
	public void ServiceAttribute_HasCorrectAttributeUsage()
	{
		// Arrange
		var attributeType = typeof(ServiceAttribute);

		// Act
		var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

		// Assert
		attributeUsage.ShouldNotBeNull();
		attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
		attributeUsage.AllowMultiple.ShouldBeFalse();
		attributeUsage.Inherited.ShouldBeFalse();
	}

	[Fact]
	public void ServiceAttribute_CanBeInstantiated()
	{
		// Arrange
		// Act
		var attribute = new ServiceAttribute(Lifetime.Scoped, typeof(ITestService));

		// Assert
		attribute.ShouldNotBeNull();
	}
}
