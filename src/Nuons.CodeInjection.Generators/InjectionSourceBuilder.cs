using System.Collections.Immutable;
using Nuons.Core.Generators;

namespace Nuons.CodeInjection.Generators;

internal class InjectionSourceBuilder
{
	private readonly string namespaceName;
	private readonly string className;
	private readonly ImmutableArray<string> typeParameterNames;
	private readonly List<string> parameters = [];
	private readonly List<string> assignments = [];

	public InjectionSourceBuilder(string namespaceName, string className, ImmutableArray<string> typeParameterNames)
	{
		this.namespaceName = namespaceName;
		this.className = className;
		this.typeParameterNames = typeParameterNames;
	}

	public void With(InjectedField field)
	{
		if (field.IsOptionsValue)
		{
			parameters.Add($"global::Microsoft.Extensions.Options.IOptions<{field.Type}> {field.Name}");
			assignments.Add($"this.{field.Name} = {field.Name}.Value;");
		}
		else
		{
			parameters.Add($"{field.Type} {field.Name}");
			assignments.Add($"this.{field.Name} = {field.Name};");
		}
	}

	public string Build()
	{
		var typeParameters = FormatTypeParameters();
		var namespaceDeclaration = string.IsNullOrEmpty(namespaceName)
			? string.Empty
			: $"namespace {namespaceName};{Sources.NewLine}";

		var source = $@"{Sources.GeneratedFileHeader}
{namespaceDeclaration}partial class {className}{typeParameters}
{{
{BuildConstructor()}
}}";

		return source;
	}

	private string BuildConstructor()
	{
		if (parameters.Count == 0)
		{
			return $"{Sources.Tab1}public {className}() {{ }}";
		}

		var allParameters = parameters.Aggregate((first, second) => $"{first},{Sources.NewLine}{Sources.Tab2}{second}");
		var allAssignments = assignments.Aggregate((first, second) => $@"{first}{Sources.NewLine}{Sources.Tab2}{second}");

		return $@"{Sources.Tab1}public {className}(
{Sources.Tab2}{allParameters})
{Sources.Tab1}{{
{Sources.Tab2}{allAssignments}
{Sources.Tab1}}}";
	}

	private string FormatTypeParameters()
	{
		if (typeParameterNames.IsDefaultOrEmpty)
		{
			return string.Empty;
		}

		return $"<{string.Join(", ", typeParameterNames)}>";
	}
}
