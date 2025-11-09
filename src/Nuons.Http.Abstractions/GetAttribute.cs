using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.Http.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class GetAttribute : Attribute
{
	public GetAttribute(string route = "") { }
}
