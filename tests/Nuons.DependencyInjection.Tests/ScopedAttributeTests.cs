using System.Reflection;

namespace Nuons.DependencyInjection.Tests;

public class ScopedAttributeTests
{
    [Fact]
    public void ScopedAttribute_HasCorrectAttributeUsage()
    {
        // Arrange
        var attributeType = typeof(ScopedAttribute);

        // Act
        var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();

        // Assert
        attributeUsage.ShouldNotBeNull();
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
        attributeUsage.AllowMultiple.ShouldBeFalse();
        attributeUsage.Inherited.ShouldBeFalse();
    }
    
    [Fact]
    public void ScopedAttribute_CanBeInstantiated()
    {
        // Arrange
        // Act
        var attribute = new ScopedAttribute(typeof(ITestService));

        // Assert
        attribute.ShouldNotBeNull();
    }
} 