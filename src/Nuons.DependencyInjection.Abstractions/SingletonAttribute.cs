using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SingletonAttribute : Attribute;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SingletonAttribute<TService> : Attribute;
