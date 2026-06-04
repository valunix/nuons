using System.Collections.Immutable;

namespace Nuons.CodeInjection.Generators;

internal record InjectionIncrement(
	string Namespace,
	string ClassName,
	ImmutableArray<string> TypeParameterNames,
	ImmutableArray<InjectedField> Fields
);
