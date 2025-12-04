namespace Nuons.DependencyInjection.Generators.Registration;

internal record CombinedRegistrationsIncrement
{
	public string? AssemblyName { get; set; }
	public bool Transient { get; set; }
	public bool Scoped { get; set; }
	public bool Singleton { get; set; }
	public bool Options { get; set; }

	public bool HasRegistrations => Transient || Scoped || Singleton;
}
