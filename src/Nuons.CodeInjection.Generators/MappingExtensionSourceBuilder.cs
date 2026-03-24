using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

internal class MappingExtensionSourceBuilder(string namespaceName, string recordName, string accessibility, string sourceFullTypeName)
{
	private readonly List<string> assignmentLines = [];

	public void With(MappedProperty property)
	{
		assignmentLines.Add($"{Sources.Tab2}{Sources.Tab1}{property.Name} = source.{property.Name},");
	}

	public string Build()
	{
		var body = assignmentLines.Count > 0
			? assignmentLines.Aggregate((first, second) => $"{first}{Sources.NewLine}{second}")
			: string.Empty;

		var initializer = assignmentLines.Count > 0
			? $"new {recordName}{Sources.NewLine}{Sources.Tab2}{{{Sources.NewLine}{body}{Sources.NewLine}{Sources.Tab2}}}"
			: $"new {recordName} {{}}";

		return $@"namespace {namespaceName};
{accessibility} static class {recordName}Extensions
{{
{Sources.Tab1}public static {recordName} To{recordName}(this {sourceFullTypeName} source)
{Sources.Tab1}{{
{Sources.Tab2}return {initializer};
{Sources.Tab1}}}
}}";
	}
}
