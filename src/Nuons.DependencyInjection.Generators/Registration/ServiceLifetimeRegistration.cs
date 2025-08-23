namespace Nuons.DependencyInjection.Generators.Registration;

record ServiceLifetimeRegistration(string ServiceType, string ImplementingType, Lifetime Lifetime)
	: ServiceRegistration(ServiceType, ImplementingType);
