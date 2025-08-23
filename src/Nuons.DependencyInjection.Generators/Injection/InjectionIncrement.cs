using System.Collections.Immutable;

namespace Nuons.DependencyInjection.Generators.Injection;

internal record InjectionIncrement(
	string Namespace,
	string ClassName,
	string Accessibility,
	ImmutableArray<InjectedField> Fields
);
