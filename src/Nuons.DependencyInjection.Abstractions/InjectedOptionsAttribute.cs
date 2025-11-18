using System.Diagnostics;
using Nuons.Core.Abstractions;

namespace Nuons.DependencyInjection.Abstractions;

[Conditional(Constants.CodeGenerationCondition)]
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class InjectedOptionsAttribute : Attribute;
