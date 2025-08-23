using System.Diagnostics;

namespace Nuons.DependencyInjection;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
// TODO is there a use case for service attribute when we have the others
public class ServiceAttribute : Attribute 
{
	public ServiceAttribute(Lifetime lifetime, Type type) { }
}