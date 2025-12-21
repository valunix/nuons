using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[Options("MyOptions")]
public class MyOptions
{
	public required string MyProperty { get; set; }
}
