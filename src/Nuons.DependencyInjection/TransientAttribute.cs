using System.Diagnostics;

namespace Nuons.DependencyInjection;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class TransientAttribute : Attribute
{
	public TransientAttribute(Type type) { }
}
