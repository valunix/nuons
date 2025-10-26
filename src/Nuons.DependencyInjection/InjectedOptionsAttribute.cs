using System.Diagnostics;

namespace Nuons.DependencyInjection;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectedOptionsAttribute : Attribute;
