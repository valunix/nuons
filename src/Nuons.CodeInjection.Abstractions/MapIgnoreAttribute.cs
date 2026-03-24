using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.CodeInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class MapIgnoreAttribute : Attribute
{
	public MapIgnoreAttribute(string propertyName) { }
}
