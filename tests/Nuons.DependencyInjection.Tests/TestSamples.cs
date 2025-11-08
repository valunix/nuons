namespace Nuons.DependencyInjection.Tests;

public interface ITestService { }

[Singleton]
public class SingletonTestService : ITestService { }

[Scoped]
public class ScopedTestService : ITestService { }

[Transient]
public class TransientTestService : ITestService { }

[Service(Lifetime.Scoped, typeof(ITestService))]
public class ServiceAttributeTestService : ITestService { }

[Options("TestSectionKey")]
public class TestOptions { }

public class TestConsumer
{
	[Injected]
	public readonly ITestService scopedService;

	[Injected]
	public readonly ITestService transientService;

	[InjectedOptions]
	public readonly TestOptions options;
}
