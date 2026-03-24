using System.Collections.Immutable;

namespace Nuons.CodeInjection.Generators;

internal record MappingIncrement(
	string TargetNamespace,
	string TargetRecordName,
	string TargetAccessibility,
	string SourceFullTypeName,
	string SourceSimpleName,
	ImmutableArray<MappedProperty> Properties
);
