namespace Nuons.DependencyInjection.Generators.Injection;

internal class InjectionSourceBuilder
{
	private readonly string namespaceName;
	private readonly string className;
	private readonly string accessibility;
	private readonly List<string> parameters = [];
	private readonly List<string> assignments = [];

	public InjectionSourceBuilder(string namespaceName, string className, string accessibility)
	{
		this.namespaceName = namespaceName;
		this.className = className;
		this.accessibility = accessibility;
	}

	public void With(InjectedField field)
	{
		if (field.IsOptionsValue)
		{
			parameters.Add($"Microsoft.Extensions.Options.IOptions<{field.Type}> {field.Name}");
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
		var allParameters = parameters.Aggregate((first, second) => $"{first},{Sources.NewLine}{Sources.Tab2}{second}");
		var allAssignments = assignments.Aggregate((first, second) => $@"{first}{Sources.NewLine}{Sources.Tab2}{second}");

		var source = $@"namespace {namespaceName};
{accessibility} partial class {className}
{{
	public {className}(
		{allParameters})
	{{
		{allAssignments}
	}}
}}";

		return source;
	}
}
