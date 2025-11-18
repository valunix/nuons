using Nuons.DependencyInjection.Abstractions;

namespace Nuons.EndToEnd.Api;

[RootRegistration(
	typeof(ScopedFeature.Infrastructure.AssemblyMarker),
	typeof(ServiceFeature.Infrastructure.AssemblyMarker),
	typeof(TransientFeature.Infrastructure.AssemblyMarker),
	typeof(SingletonFeature.Infrastructure.AssemblyMarker),
	typeof(AssemblyMarker)
)]
public partial class RootRegistration;
