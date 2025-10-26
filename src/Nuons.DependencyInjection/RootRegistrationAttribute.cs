using System.Diagnostics;

namespace Nuons.DependencyInjection;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class RootRegistrationAttribute : Attribute
{
	public RootRegistrationAttribute(params Type[] assemblyMarkerTypes) { }
}
