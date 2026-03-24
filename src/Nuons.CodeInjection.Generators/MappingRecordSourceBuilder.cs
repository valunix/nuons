using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

internal class MappingRecordSourceBuilder(string namespaceName, string recordName, string accessibility)
{
	private readonly List<string> propertyLines = [];

	public void With(MappedProperty property)
	{
		propertyLines.Add($"{Sources.Tab1}public {property.FullTypeName} {property.Name} {{ get; init; }}");
	}

	public string Build()
	{
		var body = propertyLines.Count > 0
			? propertyLines.Aggregate((first, second) => $"{first}{Sources.NewLine}{second}")
			: string.Empty;

		return $@"namespace {namespaceName};
{accessibility} partial record {recordName}
{{
{body}
}}";
	}
}
