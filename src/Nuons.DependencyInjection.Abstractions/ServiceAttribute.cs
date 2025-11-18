using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions;

// TODO is there a use case for service attribute when we have the others

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ServiceAttribute : Attribute
{
	public ServiceAttribute(Lifetime lifetime, Type type) { }
}
