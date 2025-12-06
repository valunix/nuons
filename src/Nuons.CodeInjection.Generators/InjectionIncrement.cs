using System.Collections.Immutable;

namespace Nuons.CodeInjection.Generators;

internal record InjectionIncrement(
	string Namespace,
	string ClassName,
	string Accessibility,
	ImmutableArray<InjectedField> Fields
);
