using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.Http.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RouteAttribute : Attribute
{
	public RouteAttribute(string route = "/") { }
}
