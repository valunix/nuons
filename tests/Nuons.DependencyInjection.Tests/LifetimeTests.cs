namespace Nuons.DependencyInjection.Tests;

public class LifetimeTests
{
    [Fact]
    public void Lifetime_HasCorrectValues()
    {
        // Assert
        ((int)Lifetime.Singleton).ShouldBe(0);
        ((int)Lifetime.Scoped).ShouldBe(1);
        ((int)Lifetime.Transient).ShouldBe(2);
    }
} 